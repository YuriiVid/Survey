using System;
using System.IO;
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

            public bool is_chosen = false;

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

        class Menu
        {
            Survey survey;

            int points = 0;

            int selected;

            int optionsCount;

            int chosenAnswersCount;

            public Menu(Survey survey)
            {
                this.survey = survey;
            }

            public void ChooseAnswers(int questionIndex)
            {
                Question current_question = survey.Questions[questionIndex];// тут взагалі хз чи так варто робити, я по суті перезадаю
                selected = 0;                                               // значення змінних які в класі прописані кожен раз
                optionsCount = current_question.Answers.Count;              // але не хочу їх носити в рендер через функцію, хоча напевно
                                                                            // просто вносити в функцію було б більш правильно
                chosenAnswersCount = 0;

                foreach (Answer answer in current_question.Answers) // фул костиль, але хз як інакше реалізувати повернення назад
                {
                    if (answer.is_chosen)
                        chosenAnswersCount++;
                }

                while (true)
                {
                    Render(current_question);

                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.UpArrow:
                            selected = Math.Max(0, selected - 1);
                            break;

                        case ConsoleKey.DownArrow:
                            selected = Math.Min(optionsCount - 1, selected + 1);
                            break;

                        case ConsoleKey.Spacebar:
                            if (!current_question.Answers[selected].is_chosen)
                            {
                                if (chosenAnswersCount == current_question.AnswersNumber)
                                {
                                    Console.WriteLine("You've already selected maximum number of answers");
                                    Thread.Sleep(1500);
                                    break;
                                }
                                
                                current_question.Answers[selected].is_chosen = true;
                                points += current_question.Answers[selected].Points;
                                chosenAnswersCount++;
                            }
                            else
                            {
                                chosenAnswersCount--;
                                current_question.Answers[selected].is_chosen = false;
                                points -= current_question.Answers[selected].Points;
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (questionIndex != survey.Questions.Count - 1) //хз чи норм індекси провіряю, але вже як є
                                ChooseAnswers(questionIndex + 1);
                            else
                            {
                                Console.WriteLine("You`re at last question now");
                                Thread.Sleep(1500);
                            }
                            return;

                        case ConsoleKey.LeftArrow:
                            if (questionIndex != 0) // то саме
                                ChooseAnswers(questionIndex - 1);
                            else
                            {
                                Console.WriteLine("You`re at first question now");
                                Thread.Sleep(1500);
                            }
                            return;

                        case ConsoleKey.Enter:
                            Result();
                            return;

                        default:
                            Console.WriteLine("Wrong key");
                            break;
                    }
                }
            }

            void Render(Question question)
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
                    else if (question.Answers[i].is_chosen)
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

            public void Start() // хз чи нужно ваще, але мені здалось шо так буде правильніше
            {
                while (true)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("Press Space if you want to start survey, ESC - to exit the program");
                    Console.ResetColor();

                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.Spacebar:
                            StartSurvey();
                            return;

                        case ConsoleKey.Escape:
                            Console.WriteLine("Goodbye");
                            Thread.Sleep(1500);
                            return;
                        default:
                            Console.WriteLine("Wrong key");
                            Thread.Sleep(1000);
                            break;
                    }
                }
            }

            void StartSurvey() //хз чо окрема функція, але най буде, енівей це всьо тупо якось
            {
                ChooseAnswers(0); 
            }

            public void Result()
            {
                int closest = 0;
                foreach (Mark mark in survey.Marks)
                {
                    if (points >= mark.MinimalPoints)
                        closest = survey.Marks.IndexOf(mark);
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Ur result is {points} points");
                Console.WriteLine(survey.Marks[closest].Text);
                Console.ResetColor();
                Thread.Sleep(1000);
            }
        }

        static void Main(string[] args) //зато мейн красівий
        {
            Survey survey = JsonConvert.DeserializeObject<Survey>(File.ReadAllText(PathToSurveyFile));
            Menu menu = new Menu(survey);
            menu.Start();
        }
    }
}
