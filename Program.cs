﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
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

            public bool IsChosen = false;

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

        class TestRunner
        {
            public Survey survey;

            public int questionIndex = 0;

            public TestRunner(Survey survey)
            {
                this.survey = survey;
            }

            public Question GetCurrentQuestion()
            {
                return survey.Questions[questionIndex];
            }

            public Question NextQuestion()
            {
                return survey.Questions[++questionIndex];
            }

            public Question PreviousQuestion()
            {
                return survey.Questions[--questionIndex];
            }

            public bool IsLast()
            {
                return (questionIndex == survey.Questions.Count - 1);
            }

            public bool IsFirst()
            {
                return (questionIndex == 0);
            }

            public void Result()
            {
                int points = survey.Questions.Sum(q => q.Answers.Where(a => a.IsChosen).Sum(a => a.Points));

                Mark mark = survey.Marks.First(m => m.MinimalPoints <= points);

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Your result is {points} points");
                Console.WriteLine(mark.Text);
                Console.ResetColor();
                Thread.Sleep(1000);
            }
        }

        class Render
        {
            int chosenAnswersCount;

            int optionsCount;

            int selected;

            public Render(int chosenAnswersCount, int optionsCount, int selected)
            {
                this.chosenAnswersCount = chosenAnswersCount;
                this.optionsCount = optionsCount;
                this.selected = selected;
            }

            public void DisplayQuestion(Question question)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("Press ArrowDown and ArrowUp to navigate beetween answers,");
                Console.WriteLine("Space - choose answer, RightArrow - move to the next question,");
                Console.WriteLine("LeftArrow - move to the previous question, Enter - submit survey");

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[You've selected {chosenAnswersCount}/{question.AnswersNumber} answers]");

                Console.ResetColor();
                Console.WriteLine(question.Text);

                for (int i = 0; i < optionsCount; i++)
                {
                    if (selected == i)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("> ");
                    }
                    else if (question.Answers[i].IsChosen)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("+ ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Console.WriteLine(question.Answers[i].Text);

                    Console.ResetColor();
                }
            }
        }

        static void Main(string[] args) //зато мейн красівий
        {
            Survey survey = JsonConvert.DeserializeObject<Survey>(File.ReadAllText(PathToSurveyFile));

            TestRunner testRunner = new TestRunner(survey);

            // while (true)
            // {
            //     Console.Clear();
            //     Console.ForegroundColor = ConsoleColor.DarkMagenta;
            //     Console.WriteLine("Press Space if you want to start survey, ESC - to exit the program");
            //     Console.ResetColor();

            //     switch (Console.ReadKey(intercept: true).Key)
            //     {
            //         case ConsoleKey.Spacebar:
            //             StartSurvey();
            //             break;

            //         case ConsoleKey.Escape:
            //             Console.WriteLine("Goodbye");
            //             Thread.Sleep(1500);
            //             break;
            //         default:
            //             Console.WriteLine("Wrong key");
            //             Thread.Sleep(1000);
            //             break;
            //     }
            // }
            Question question = testRunner.GetCurrentQuestion();

            int selected = 0;

            while (true)
            {
                int chosenAnswersCount = question.Answers.Count(a => a.IsChosen);

                int optionsCount = question.Answers.Count;

                Render render = new Render(chosenAnswersCount, optionsCount, selected);

                render.DisplayQuestion(question);

                switch (Console.ReadKey(intercept: true).Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = Math.Max(0, selected - 1);
                        break;

                    case ConsoleKey.DownArrow:
                        selected = Math.Min(optionsCount - 1, selected + 1);
                        break;

                    case ConsoleKey.Spacebar:
                        if (!question.Answers[selected].IsChosen)
                        {
                            if (chosenAnswersCount == question.AnswersNumber)
                            {
                                Console.WriteLine("You've already selected maximum number of answers");
                                Thread.Sleep(1500);
                                break;
                            }
                            question.Answers[selected].IsChosen = true;
                        }
                        else
                        {
                            question.Answers[selected].IsChosen = false;
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (!testRunner.IsLast())
                        {
                            question = testRunner.NextQuestion();
                            selected = 0;
                        }
                        else
                        {
                            Console.WriteLine("You`re at last question now");
                            Thread.Sleep(1500);
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (!testRunner.IsFirst())
                        {
                            question = testRunner.PreviousQuestion();
                            selected = 0;
                        }
                        else
                        {
                            Console.WriteLine("You`re at first question now");
                            Thread.Sleep(1500);
                        }
                        break;

                    case ConsoleKey.Enter:
                        testRunner.Result();
                        return;

                    default:
                        Console.WriteLine("Wrong key");
                        break;
                }
            }
        }
    }
}