using Godot;
using System;
using System.Collections.Generic;
using Game.Data;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game.Autoloads;

public partial class QuestionManager : Autoload<QuestionManager>
{
    [Signal] public delegate void QuestionRequestedEventHandler(Question question);

    public Question CurrentQuestion { get; private set; }
    private List<Question> questions = [];
    private int currentIndex = -1;

    public void LoadQuestions(Question[] question)
    {
        questions = new List<Question>(question);
        for (var i = questions.Count - 1; i > 0; i--)
        {
            var j = MathUtil.RNG.RandiRange(0, i);
            (questions[i], questions[j]) = (questions[j], this.questions[i]);
        }

        currentIndex = -1;
    }

    public Question GetNextQuestion()
    {
        if (questions.Count == 0)
        {
            Logger.Error("No questions loaded!");
            return null;
        }

        currentIndex++;
        if (currentIndex >= questions.Count)
        {
            Logger.Error("No more questions available.");
            return null;
        }

        CurrentQuestion = questions[currentIndex];
        EmitSignal(SignalName.QuestionRequested, CurrentQuestion);
        return CurrentQuestion;
    }

    public bool IsAnswerCorrect(int answerIndex)
        => CurrentQuestion != null && answerIndex == CurrentQuestion.CorrectAnswerIndex;
}