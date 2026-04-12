using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TheGatekeeper.Models
{
    [DataContract]
    public class SaveData
    {
        [DataMember] public int Score { get; set; }
        [DataMember] public int Health { get; set; }
        [DataMember] public int Day { get; set; }
        [DataMember] public int Level { get; set; }
        [DataMember] public DateTime SaveTime { get; set; }
        [DataMember] public string PlayerName { get; set; }
    }

    [DataContract]
    public class GameResult
    {
        [DataMember] public DateTime Date { get; set; }
        [DataMember] public int FinalScore { get; set; }
        [DataMember] public int DaysSurvived { get; set; }
        [DataMember] public int MaxLevel { get; set; }
        [DataMember] public bool IsVictory { get; set; }
    }
}