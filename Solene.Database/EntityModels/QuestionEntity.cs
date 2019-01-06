using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;

namespace Solene.Database.EntityModels
{
    internal class QuestionEntity : TableEntity
    {
        public QuestionEntity() { }

        internal QuestionEntity(Question question)
        {
            PartitionKey = PartitionKeys.Question;
            RowKey = question.Id.ToString("N");
            PlayerId = question.PlayerId;
            Text = question.Text;
            PrefilledAnswersJson = JsonConvert.SerializeObject(question.PrefilledAnswers);
            SequenceNumber = (int)question.SequenceNumber;
            ChosenAnswer = question.ChosenAnswer;
        }

        public Guid PlayerId { get; set; }
        public string Text { get; set; }
        public string PrefilledAnswersJson { get; set; }
        public int SequenceNumber { get; set; }
        public string ChosenAnswer { get; set; }

        public Question ToQuestion()
        {
            return new Question
            {
                Id = Guid.Parse(RowKey),
                PlayerId = PlayerId,
                Text = Text,
                PrefilledAnswers = JsonConvert.DeserializeObject<List<string>>(PrefilledAnswersJson),
                ChosenAnswer = ChosenAnswer,
                SequenceNumber = (uint)SequenceNumber,
            };
        }
    }
}
