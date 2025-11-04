using Godot;
using System;
using Game.Data;

namespace Game.Data;

[GlobalClass]
public partial class LevelInfo : Resource
{
    [Export] public string LevelName;
    [Export(PropertyHint.MultilineText)] public string LevelDescription;
    [Export] public RunConfig.SubjectArea Subject;
    [Export] public RunConfig.GradeLevel Grade;
    [Export] public CombatSequence CombatSequence;
    [Export] public int StarCount;

    [Export] public Question[] Questions;
}