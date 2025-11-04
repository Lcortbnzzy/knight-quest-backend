using Godot;

namespace Game.Data;

[Tool, GlobalClass]
public partial class Question : Resource
{
    [Export] public string QuestionText;
    [Export] public string[] Answers = [];

    [Export]
    public int CorrectAnswerIndex
    {
        get => _correctAnswerIndex;
        set
        {
            if (value < 0 || value >= Answers.Length) return;

            _correctAnswerIndex = value;
            NotifyPropertyListChanged();
        }
    }

    [Export]
    public string CorrectAnswer
    {
        get => Answers.Length > 1 ? Answers[CorrectAnswerIndex] : string.Empty;
        set { }
    }

    private int _correctAnswerIndex;
}