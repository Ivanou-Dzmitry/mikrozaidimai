using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Drag-and-release aiming for BubbleField's current projectile — classic
// slingshot: drag the ball down and to a side, release, it flies the mirrored
// direction (pull down-left -> launches up-right). The pull angle is clamped
// to +/- field.MaxAimAngle degrees from straight down (that field owns the
// angle since it also uses it to only ever offer reachable targets — keep
// this in sync by reading from there rather than a local copy). Z stays out
// of it entirely — this only ever moves/aims in the board's XY plane.
// Uses Pointer (base class shared by Mouse and Touchscreen) so it works the
// same whether tested with a mouse, through Device Simulator's touch emulation,
// or on an actual phone.
public class ShooterAimController : MonoBehaviour
{
    [SerializeField] BubbleField field;
    [SerializeField] float dragSensitivity = 0.01f;
    [SerializeField] float maxPullDistance = 3f;
    [Tooltip("Below this pull distance, aim angle holds at straight down instead of being computed — atan2 is numerically unstable near zero pull (tiny jitter swings the angle wildly).")]
    [SerializeField] float angleDeadzone = 0.15f;
    [Tooltip("How long the aim angle takes to catch up to the target angle. 0 = instant/no smoothing.")]
    [SerializeField] float angleSmoothTime = 0.06f;
    [SerializeField] AimLine aimLine;
    [SerializeField] float aimLinePreviewLength = 5f;
    [SerializeField] int aimLineMaxBounces = 2;
    [SerializeField] SpringVisual spring;

    [Header("Launch force (spring feel)")]
    [SerializeField] float minLaunchForce = 2f;
    [SerializeField] float maxLaunchForce = 15f;
    [Tooltip("Maps pull fraction (0 = no pull, 1 = maxPullDistance) to force fraction between min and max.")]
    [SerializeField] AnimationCurve pullCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] AudioClip launchSound;

    bool dragging;
    Vector2 dragStartScreen;
    float smoothedAngle;
    float angleSmoothVelocity;

    void Update()
    {
        if (field == null) return;

        // don't aim/shoot while the game isn't actually playing (e.g. Settings
        // panel open and the session paused) or while there's nothing to shoot
        if (field.Session != null && field.Session.State != GameState.Play)
        {
            dragging = false;
            if (aimLine != null) aimLine.Hide();
            if (spring != null) spring.ReleaseSpring();
            if (field.CurrentProjectile != null) field.CurrentProjectile.SetLabelLifted(false);
            return;
        }

        ShooterProjectile projectile = field.CurrentProjectile;
        if (projectile == null || projectile.Launched)
        {
            if (aimLine != null) aimLine.Hide();
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverInteractiveUI(pointer.position.ReadValue())) return; // a real button (settings, info, etc.), not just background UI

            dragging = true;
            dragStartScreen = pointer.position.ReadValue();
            smoothedAngle = 0f;
            angleSmoothVelocity = 0f;
            projectile.SetLabelLifted(true);
        }

        if (dragging && pointer.press.isPressed)
        {
            Vector2 clampedOffset = ComputeClampedPullOffset(pointer.position.ReadValue());

            // the ball model itself only ever pulls straight down — the sideways
            // component of the aim only shows up in the launch vector and the
            // aim line below, not in how the model visually moves while dragging
            Vector3 pos = field.ShooterOrigin;
            pos.y += clampedOffset.y;
            projectile.transform.position = pos;

            if (spring != null) spring.SetCompression(clampedOffset.magnitude / maxPullDistance);

            if (aimLine != null)
            {
                Vector2 dir = clampedOffset.sqrMagnitude > 0.0001f ? -clampedOffset.normalized : Vector2.up;
                aimLine.Show(ComputeAimPath(pos, dir, projectile).ToArray());
            }
        }

        if (dragging && pointer.press.wasReleasedThisFrame)
        {
            dragging = false;
            if (aimLine != null) aimLine.Hide();
            if (HelperPanel.Instance != null) HelperPanel.Instance.HideImmediately();
            if (spring != null) spring.ReleaseSpring();
            projectile.SetLabelLifted(false);

            Vector2 clampedOffset = ComputeClampedPullOffset(pointer.position.ReadValue());
            float pullFraction = Mathf.Clamp01(clampedOffset.magnitude / maxPullDistance);
            float force = Mathf.Max(minLaunchForce, Mathf.Lerp(minLaunchForce, maxLaunchForce, pullCurve.Evaluate(pullFraction)));

            // mirrored: launch opposite to the direction it was pulled in
            Vector2 launchDir2D = clampedOffset.sqrMagnitude > 0.0001f ? -clampedOffset.normalized : Vector2.up;
            projectile.Launch(new Vector3(launchDir2D.x, launchDir2D.y, 0f) * force);

            if (launchSound != null) AudioManager.Instance.PlaySfx(launchSound);
        }
    }

    // True only if this screen position is over an actually-clickable UI control
    // (Button, Toggle, Slider, ...) — NOT just any raycast-blocking background
    // Image (e.g. CenterPanel's own backdrop over the game field), which would
    // otherwise make every click on the ball look like it's "over UI" too.
    static readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    static bool IsPointerOverInteractiveUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);

        foreach (RaycastResult result in uiRaycastResults)
        {
            if (result.gameObject.GetComponentInParent<Selectable>() != null) return true;
        }

        return false;
    }

    // Predicts the shot's actual path, bending it at each wall bounce (mirror
    // reflection, same as the real physics) so the guide shows where it'll
    // really end up, not just the initial direction. Stops early if it reaches
    // a WordBubble target — that's where the shot would actually land.
    List<Vector3> ComputeAimPath(Vector3 start, Vector2 direction2D, ShooterProjectile projectile)
    {
        var points = new List<Vector3> { start };

        SphereCollider sphere = projectile.GetComponent<SphereCollider>();
        float radius = sphere != null ? sphere.radius * projectile.transform.lossyScale.x : 0.1f;

        Vector3 dir = new Vector3(direction2D.x, direction2D.y, 0f).normalized;
        // start just past the ball's own surface so the cast can't immediately hit itself
        Vector3 pos = start + dir * (radius + 0.02f);
        float remaining = aimLinePreviewLength;

        for (int bounce = 0; bounce <= aimLineMaxBounces && remaining > 0f; bounce++)
        {
            if (Physics.SphereCast(pos, radius, dir, out RaycastHit hit, remaining))
            {
                points.Add(hit.point);
                remaining -= hit.distance;
                pos = hit.point;

                if (hit.collider.GetComponentInParent<WordBubble>() != null) break;

                dir = Vector3.Reflect(dir, hit.normal);
            }
            else
            {
                points.Add(pos + dir * remaining);
                break;
            }
        }

        return points;
    }

    // World-space (X,Y) offset from the shooter origin for the current drag,
    // constrained to straight-down +/- maxAimAngle and clamped to maxPullDistance.
    Vector2 ComputeClampedPullOffset(Vector2 currentScreenPos)
    {
        Vector2 screenDelta = currentScreenPos - dragStartScreen;
        Vector2 rawOffset = screenDelta * dragSensitivity;

        if (rawOffset.y > 0f) rawOffset.y = 0f; // never allow pulling above the origin

        float magnitude = Mathf.Min(rawOffset.magnitude, maxPullDistance);

        // angle measured from straight down (0,-1); positive = tilted toward +X.
        // Below the deadzone, atan2 is numerically unstable (y near 0 means tiny
        // jitter on x swings the angle wildly) so just hold at straight down.
        float targetAngle = magnitude < angleDeadzone
            ? 0f
            : Mathf.Clamp(Mathf.Atan2(rawOffset.x, -rawOffset.y) * Mathf.Rad2Deg, -field.MaxAimAngle, field.MaxAimAngle);

        smoothedAngle = Mathf.SmoothDampAngle(smoothedAngle, targetAngle, ref angleSmoothVelocity, angleSmoothTime);

        float rad = smoothedAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)) * magnitude;
    }
}
