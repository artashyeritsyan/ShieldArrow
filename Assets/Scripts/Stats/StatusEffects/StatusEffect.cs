public abstract class StatusEffect
{
    public float duration;
    public float tickInterval = 1f;

    public virtual void OnApply() { }
    public virtual void OnTick() { }
    public virtual void OnExpire() { }
}