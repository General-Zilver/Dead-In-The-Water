using UnityEngine;

public class PlayerGrappleState : MonoBehaviour
{
    public enum GrappleMode
    {
        None,
        JellyfishPull,
        SawSharkRide
    }

    public GrappleMode CurrentMode { get; private set; } = GrappleMode.None;

    public bool CanStartGrapple()
    {
        return CurrentMode == GrappleMode.None;
    }

    public bool TryStartJellyfishPull()
    {
        if (!CanStartGrapple())
            return false;

        CurrentMode = GrappleMode.JellyfishPull;
        return true;
    }

    public bool TryStartSawSharkRide()
    {
        if (!CanStartGrapple())
            return false;

        CurrentMode = GrappleMode.SawSharkRide;
        return true;
    }

    public void EndGrapple()
    {
        CurrentMode = GrappleMode.None;
    }
}
