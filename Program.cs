using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Survey
{
    class Program
    {
        class Survey
        {
            [JsonProperty(PropertyName = "questions")]
            public List<Question> Questions;

            [JsonProperty(PropertyName = "marks")]
            public List<Mark> Marks;

            public Survey(List<Question> Questions, List<Mark> Marks)
            {
                this.Questions = Questions;
                this.Marks = Marks;
            }
        }

        class Question
        {
            [JsonProperty(PropertyName = "text")]
            public string Text { get; }

            [JsonProperty(PropertyName = "answers_number")]
            public int AnswersNumber;

            [JsonProperty(PropertyName = "answers")]
            public List<Answer> Answers { get; }

            public Question(string Text, int AnswersNumber, List<Answer> Answers)
            {
                this.Text = Text;
                this.AnswersNumber = AnswersNumber;
                this.Answers = Answers;
            }
        }

        class Answer
        {
            [JsonProperty(PropertyName = "text")]
            public string Text { get; }

            [JsonProperty(PropertyName = "points")]
            public int Points { get; }

            public Answer(string Text, int Points)
            {
                this.Text = Text;
                this.Points = Points;
            }
        }

        class Mark
        {
            [JsonProperty(PropertyName = "text")]
            public string Text { get; }

            [JsonProperty(PropertyName = "minimal_points")]
            public int MinimalPoints { get; }

            public Mark(string Text, int MinimalPoints)
            {
                this.Text = Text;
                this.MinimalPoints = MinimalPoints;
            }
        }

        const string PathToSurveyFile = "Survey.json";

        static int ChooseAnswersAndGetPoints(List<Answer> answers, int answers_number)
        {
            int selected = 0, points = 0;
            int optionsCount = answers.Count;
            bool done = false;

            List<int> RightAnswers = new List<int>();

            while (!done)
            {
                for (int i = 0; i < optionsCount; i++)
                {
                    if (selected == i || RightAnswers.Contains(i))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Console.WriteLine(answers[i].Text);

                    Console.ResetColor();
                }

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = Math.Max(0, selected - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selected = Math.Min(optionsCount - 1, selected + 1);
                        break;
                    case ConsoleKey.Spacebar:
                        if (!RightAnswers.Contains(selected))
                        {
                            RightAnswers.Add(selected);
                            points += answers[selected].Points;
                            if (answers_number == 1)
                            {
                                done = true;
                            }
                            else
                            {
                                answers_number -= 1;
                            }
                        }
                        else
                        {
                            RightAnswers.Remove(selected);
                            points -= answers[selected].Points;
                            answers_number += 1;
                        }
                        break;
                }

                if (!done)
                    Console.CursorTop -= optionsCount;
            }
            return points;
        }

        static void Main(string[] args)
        {
            Survey survey = JsonConvert.DeserializeObject<Survey>(File.ReadAllText(PathToSurveyFile));
            int points = 0;
            foreach (Question question in survey.Questions)
            {
                Console.WriteLine(question.Text);

                points += ChooseAnswersAndGetPoints(question.Answers, question.AnswersNumber);
            }

            int closest = 0;
            foreach (Mark mark in survey.Marks)
            {
                if (points > mark.MinimalPoints)
                    closest = survey.Marks.IndexOf(mark);
            }
            Console.WriteLine($"Ur result is {points} points");
            Console.WriteLine(survey.Marks[closest].Text);
        }
    }
}
