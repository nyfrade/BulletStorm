namespace BulletStorm
{
    // Enum que representa os diferentes estados do jogo
    public enum GameState
    {
        SafeHouse,      // Hub de gerenciamento
        CombatPhase1,   // Fase de combate com inimigos básicos
        WeaponChoice,   // Escolha de arma
        CombatPhase3,   // Mini-boss ou boss
        LevelComplete   // Fase concluída
    }
}
