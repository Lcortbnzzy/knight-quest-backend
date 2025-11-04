using Game.Data;
using Godot;


namespace Game.Data;

public partial class RunConfig : RefCounted
{
    public enum GradeLevel
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth,
    }

    public enum SubjectArea
    {
        English,
        Mathematics,
    }

    public GradeLevel Grade;
    public SubjectArea Subject;
    public Character Character;
    public LevelInfo Level;
}