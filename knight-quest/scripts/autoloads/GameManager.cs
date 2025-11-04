using Game.Data;

namespace Game.Autoloads;

public partial class GameManager : Autoload<GameManager>
{
    private RunConfig config = new();
    public static RunConfig Config => Instance.config;

    public static void SetGradeLevel(RunConfig.GradeLevel level)
    {
        Instance.config.Grade = level;
    }

    public static void SetSubjectArea(RunConfig.SubjectArea area)
    {
        Instance.config.Subject = area;
    }

    public static void SetCharacter(Character character)
    {
        Instance.config.Character = character;
    }

    public static void SetLevel(LevelInfo level)
    {
        Instance.config.Level = level;
    }
}