namespace TheGatekeeper.Models
{
    public enum GameMode
    {
        StoryMode = 0,   // Бывший DailyQuota  → «ПРОТОКОЛ ВРАТА» (сюжет, 10 дней, концовки)
        HuntMode = 1,   // Бывший FindTheVillain → «ОХОТА» (злодей среди толпы)
        EndlessMode = 2    // Бывший CriticalMission → «БЕСКОНЕЧНАЯ СМЕНА»
    }
}