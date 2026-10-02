// One fighter in a battle: a party hero or a copy of an enemy.
// Battle changes CurrentHp here; HeroData and EnemyData are never changed during a fight
public class BattleUnit
{
    public string Name { get; }
    public HeroData Hero { get; }   // null for enemies
    public int MaxHp { get; }
    public int CurrentHp { get; set; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }

    public bool IsHero => Hero != null;
    public bool IsAlive => CurrentHp > 0;
    public bool IsHealer => Hero != null && Hero.Class == HeroClass.Healer;

    private BattleUnit(string name, HeroData hero, int maxHp, int currentHp, int attack, int defense, int speed)
    {
        Name = name;
        Hero = hero;
        MaxHp = maxHp;
        CurrentHp = currentHp;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }

    // Base stats + weapon (Attack) + hat (Defense) + relationship tier bonus (both)
    public static BattleUnit FromHero(HeroData hero, int tierBonus)
    {
        int weaponBonus = hero.Weapon != null ? hero.Weapon.StatBonus : 0;
        int hatBonus = hero.Hat != null ? hero.Hat.StatBonus : 0;
        return new BattleUnit(hero.Name, hero, hero.MaxHp, hero.CurrentHp,
            hero.Attack + weaponBonus + tierBonus, hero.Defense + hatBonus + tierBonus, hero.Speed);
    }

    public static BattleUnit FromEnemy(EnemyData enemy, string name)
    {
        return new BattleUnit(name, null, enemy.MaxHp, enemy.MaxHp, enemy.Attack, enemy.Defense, enemy.Speed);
    }
}