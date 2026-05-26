using Xunit;
using Moq;
using System.Data;
using System.Threading.Tasks;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.ApplicationCore.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CommUnityApp.UnitTests
{
    public class QuizGameRepositoryTests
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbTransaction> _mockTransaction;
        private readonly Mock<IDapperWrapper> _mockDapper;
        private readonly QuizGameRepository _repository;

        public QuizGameRepositoryTests()
        {
            _mockConnection = new Mock<IDbConnection>();
            _mockTransaction = new Mock<IDbTransaction>();
            _mockDapper = new Mock<IDapperWrapper>();

            _mockConnection.Setup(c => c.BeginTransaction()).Returns(_mockTransaction.Object);
            _mockConnection.Setup(c => c.Open());

            Func<IDbConnection> connectionFactory = () => _mockConnection.Object;
            _repository = new QuizGameRepository(connectionFactory, _mockDapper.Object);
        }

        private void SetupQueryFirstOrDefaultAsyncForBaseResponse(string spName, BaseResponse result)
        {
            _mockDapper
                .Setup(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                    It.IsAny<IDbConnection>(),
                    It.Is<string>(s => s.Contains(spName)),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task AddUpdateQuizGameAsync_NewQuiz_Success()
        {
            // Arrange
            var request = new AddUpdateQuizGameRequest
            {
                QuizId = 0,
                BusinessId = 1,
                GameName = "Test Quiz",
                Description = "A quiz for testing",
                CreatedByAdminId = 1,
                IsActive = true,
                Config = new QuizGameConfigRequest
                {
                    ConfigId = 0,
                    MaxAttemptsPerDay = 5,
                    QuestionsPerQuiz = 10,
                    PassingScore = 70,
                    GameStartDate = DateTime.Now,
                    GameEndDate = DateTime.Now.AddDays(30)
                },
                Questions = new List<QuizQuestionRequest>
                {
                    new QuizQuestionRequest
                    {
                        QuestionId = 0,
                        QuestionText = "What is 2+2?",
                        Points = 10,
                        Options = new List<QuizOptionRequest>
                        {
                            new QuizOptionRequest { OptionId = 0, OptionText = "4", IsCorrect = true },
                            new QuizOptionRequest { OptionId = 0, OptionText = "5", IsCorrect = false }
                        }
                    }
                }
            };

            // Mock SP calls
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizGameConfig", new BaseResponse { ResultId = 101, ResultMessage = "Config added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizGame", new BaseResponse { ResultId = 1, ResultMessage = "Quiz added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizQuestion", new BaseResponse { ResultId = 201, ResultMessage = "Question added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizOption", new BaseResponse { ResultId = 301, ResultMessage = "Option added." });

            // Act
            var result = await _repository.AddUpdateQuizGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ResultId > 0);
            Assert.Equal("Quiz added.", result.ResultMessage);

            _mockTransaction.Verify(t => t.Commit(), Times.Once);
            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object, "sp_AddQuizGameConfig", It.IsAny<object>(), _mockTransaction.Object, null, CommandType.StoredProcedure), Times.Once);
            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object, "sp_AddQuizGame", It.IsAny<object>(), _mockTransaction.Object, null, CommandType.StoredProcedure), Times.Once);
            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object, "sp_AddQuizQuestion", It.IsAny<object>(), _mockTransaction.Object, null, CommandType.StoredProcedure), Times.Once);
            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object, "sp_AddQuizOption", It.IsAny<object>(), _mockTransaction.Object, null, CommandType.StoredProcedure), Times.Exactly(2));
        }

        [Fact]
        public async Task AddUpdateQuizGameAsync_FailureOnQuestionAdd_RollsBack()
        {
            // Arrange
            var request = new AddUpdateQuizGameRequest
            {
                QuizId = 0,
                BusinessId = 1,
                GameName = "Failing Quiz",
                Config = new QuizGameConfigRequest { ConfigId = 0 },
                Questions = new List<QuizQuestionRequest>
                {
                    new QuizQuestionRequest { QuestionId = 0, QuestionText = "Fail me" }
                }
            };

            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizGameConfig", new BaseResponse { ResultId = 101 });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizGame", new BaseResponse { ResultId = 1 });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddQuizQuestion", new BaseResponse { ResultId = 0, ResultMessage = "Failed to add question." });

            // Act
            var result = await _repository.AddUpdateQuizGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ResultId);
            Assert.Equal("Failed to add question.", result.ResultMessage);

            _mockTransaction.Verify(t => t.Commit(), Times.Never);
            _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        }
    }
}
