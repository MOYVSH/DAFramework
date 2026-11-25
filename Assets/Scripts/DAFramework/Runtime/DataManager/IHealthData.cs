public interface IHealthData
{
    public int EntityID { get;}
    public float CurrentHp { get; set; }
    public float CurrentAP { get; set; }
    public float MaxHP { get; set; }
    public float MaxAP { get; set; }
    public bool IsBrokeArmor { get; set; }
    public bool IsDead { get; set; }
}
