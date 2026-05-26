using Xunit;
using Moq;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CommUnityApp.InfrastructureLayer.Repositories;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System;
using CommUnityApp.ApplicationCore.Interfaces;

namespace CommUnityApp.UnitTests
{
    public class SpinGameRepositoryTests
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbTransaction> _mockTransaction;
        private readonly Mock<IDapperWrapper> _mockDapper;
        private readonly SpinGameRepository _repository;

        public SpinGameRepositoryTests()
        {
            _mockConnection = new Mock<IDbConnection>();
            _mockTransaction = new Mock<IDbTransaction>();
            _mockDapper = new Mock<IDapperWrapper>();

            // Setup mock connection behavior
            _mockConnection.Setup(c => c.BeginTransaction()).Returns(_mockTransaction.Object);
            _mockConnection.Setup(c => c.Open());
            _mockConnection.Setup(c => c.Close());
            _mockConnection.Setup(c => c.Dispose());

            // Create a factory that returns our mock connection
            Func<IDbConnection> connectionFactory = () => _mockConnection.Object;

            // Instantiate repository with the mock factory and mock DapperWrapper
            _repository = new SpinGameRepository(connectionFactory, _mockDapper.Object);
        }

        // Helper method to simulate Dapper's QueryFirstOrDefaultAsync for BaseResponse
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

        // Helper method to simulate Dapper's ExecuteAsync
        private void SetupExecuteAsync(string sql, int rowsAffected)
        {
            _mockDapper
                .Setup(d => d.ExecuteAsync(
                    It.IsAny<IDbConnection>(),
                    It.Is<string>(s => s.Contains(sql)),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(rowsAffected);
        }


        [Fact]
        public async Task AddUpdateSpinGameAsync_NewGame_Success()
        {
            // Arrange
            var request = new AddUpdateSpinGameRequest
            {
                GameId = 0,
                BusinessId = 1,
                GameName = "Test Spin Game",
                Description = "A game for testing",
                CreatedByAdminId = 1,
                IsActive = true,
                Configs = new List<SpinGameConfigRequest>
                {
                    new SpinGameConfigRequest
                    {
                        ConfigId = 0,
                        MaxSpinsPerDay = 1,
                        NumberOfSections = 3,
                        GameStartDate = DateTime.Now,
                        GameEndDate = DateTime.Now.AddDays(7),
                        IsActive = true
                    }
                },
                Sections = new List<SpinSectionRequest>
                {
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 1, PrizeText = "Win 10 Points", Points = 10, Color = "#FF0000" },
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 2, PrizeText = "Try Again", Color = "#00FF00" },
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 3, PrizeText = "Win Promotion", PromotionId = 1, Color = "#0000FF" }
                }
            };

            // Mock Dapper calls
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGame", new BaseResponse { ResultId = 1, ResultMessage = "Game added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGameConfig", new BaseResponse { ResultId = 101, ResultMessage = "Config added." });
            SetupExecuteAsync("UPDATE SpinGame SET ConfigId", 1); // For updating ConfigId in SpinGame
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinSection", new BaseResponse { ResultId = 201, ResultMessage = "Section added." });

            // Act
            var result = await _repository.AddUpdateSpinGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ResultId > 0);
            Assert.Equal("Game added.", result.ResultMessage);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(t => t.Commit(), Times.Once);
            _mockTransaction.Verify(t => t.Rollback(), Times.Never);

            // Verify stored procedure calls on _mockDapper
            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_AddSpinGame",
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 0 && (int)p.GetType().GetProperty("CreatedByAdminId").GetValue(p) == 1),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Once);

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_AddSpinGameConfig",
                It.Is<object>(p => (int)p.GetType().GetProperty("ConfigId").GetValue(p) == 0),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Once);

            _mockDapper.Verify(d => d.ExecuteAsync(
                _mockConnection.Object,
                It.Is<string>(sql => sql.Contains("UPDATE SpinGame SET ConfigId")),
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 1 && (int)p.GetType().GetProperty("ConfigId").GetValue(p) == 101),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()), Times.Once);

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_AddSpinSection",
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 1),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Exactly(3));
        }

        [Fact]
        public async Task AddUpdateSpinGameAsync_UpdateGame_Success()
        {
            // Arrange
            var request = new AddUpdateSpinGameRequest
            {
                GameId = 1, // Existing game
                BusinessId = 1,
                GameName = "Updated Spin Game",
                Description = "An updated game for testing",
                ConfigId = 101, // Existing config
                CreatedByAdminId = 1,
                IsActive = true,
                Configs = new List<SpinGameConfigRequest>
                {
                    new SpinGameConfigRequest
                    {
                        ConfigId = 101,
                        MaxSpinsPerDay = 2,
                        NumberOfSections = 4,
                        GameStartDate = DateTime.Now.AddDays(-1),
                        GameEndDate = DateTime.Now.AddDays(6),
                        IsActive = true
                    }
                },
                Sections = new List<SpinSectionRequest>
                {
                    new SpinSectionRequest { SectionId = 201, GameId = 1, SectionNumber = 1, PrizeText = "Updated 10 Points", Points = 10, Color = "#FF0000" },
                    new SpinSectionRequest { SectionId = 202, GameId = 1, SectionNumber = 2, PrizeText = "Updated Try Again", Color = "#00FF00" },
                    new SpinSectionRequest { SectionId = 0, GameId = 1, SectionNumber = 4, PrizeText = "New Section", Color = "#FFFF00" } // New section
                }
            };

            // Mock Dapper calls
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_UpdateSpinGame", new BaseResponse { ResultId = 1, ResultMessage = "Game updated." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_UpdateSpinGameConfig", new BaseResponse { ResultId = 101, ResultMessage = "Config updated." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_UpdateSpinSection", new BaseResponse { ResultId = 201, ResultMessage = "Section updated." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinSection", new BaseResponse { ResultId = 203, ResultMessage = "New section added." });


            // Act
            var result = await _repository.AddUpdateSpinGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ResultId > 0);
            Assert.Equal("Game updated.", result.ResultMessage);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(t => t.Commit(), Times.Once);
            _mockTransaction.Verify(t => t.Rollback(), Times.Never);

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_UpdateSpinGame",
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 1 && (int)p.GetType().GetProperty("CreatedByAdminId").GetValue(p) == 1),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Once);

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_UpdateSpinGameConfig",
                It.Is<object>(p => (int)p.GetType().GetProperty("ConfigId").GetValue(p) == 101),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Once);

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_UpdateSpinSection",
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 1),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Exactly(2)); // Two existing sections updated

            _mockDapper.Verify(d => d.QueryFirstOrDefaultAsync<BaseResponse>(
                _mockConnection.Object,
                "sp_AddSpinSection",
                It.Is<object>(p => (int)p.GetType().GetProperty("GameId").GetValue(p) == 1),
                _mockTransaction.Object,
                It.IsAny<int?>(),
                CommandType.StoredProcedure), Times.Once); // One new section added
        }

        [Fact]
        public async Task AddUpdateSpinGameAsync_NewGame_FailureOnGameAdd_RollsBack()
        {
            // Arrange
            var request = new AddUpdateSpinGameRequest
            {
                GameId = 0,
                BusinessId = 1,
                GameName = "Test Spin Game",
                Description = "A game for testing",
                CreatedByAdminId = 1,
                IsActive = true,
                Configs = new List<SpinGameConfigRequest>
                {
                    new SpinGameConfigRequest
                    {
                        ConfigId = 0,
                        MaxSpinsPerDay = 1,
                        NumberOfSections = 3,
                        GameStartDate = DateTime.Now,
                        GameEndDate = DateTime.Now.AddDays(7),
                        IsActive = true
                    }
                },
                Sections = new List<SpinSectionRequest>
                {
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 1, PrizeText = "Win 10 Points", Points = 10, Color = "#FF0000" }
                }
            };

            // Mock Dapper calls - Game add fails
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGame", new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update SpinGame." }); // Corrected message

            // Act
            var result = await _repository.AddUpdateSpinGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ResultId > 0);
            Assert.Equal("Failed to add/update SpinGame.", result.ResultMessage);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(t => t.Commit(), Times.Never);
            _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        }

        [Fact]
        public async Task AddUpdateSpinGameAsync_NewGame_FailureOnConfigAdd_RollsBack()
        {
            // Arrange
            var request = new AddUpdateSpinGameRequest
            {
                GameId = 0,
                BusinessId = 1,
                GameName = "Test Spin Game",
                Description = "A game for testing",
                CreatedByAdminId = 1,
                IsActive = true,
                Configs = new List<SpinGameConfigRequest>
                {
                    new SpinGameConfigRequest
                    {
                        ConfigId = 0,
                        MaxSpinsPerDay = 1,
                        NumberOfSections = 3,
                        GameStartDate = DateTime.Now,
                        GameEndDate = DateTime.Now.AddDays(7),
                        IsActive = true
                    }
                },
                Sections = new List<SpinSectionRequest>
                {
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 1, PrizeText = "Win 10 Points", Points = 10, Color = "#FF0000" }
                }
            };

            // Mock Dapper calls - Game add succeeds, Config add fails
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGame", new BaseResponse { ResultId = 1, ResultMessage = "Game added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGameConfig", new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update SpinGame Configuration." }); // Corrected message

            // Act
            var result = await _repository.AddUpdateSpinGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ResultId > 0);
            Assert.Equal("Failed to add/update SpinGame Configuration.", result.ResultMessage);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(t => t.Commit(), Times.Never);
            _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        }

        [Fact]
        public async Task AddUpdateSpinGameAsync_NewGame_FailureOnSectionAdd_RollsBack()
        {
            // Arrange
            var request = new AddUpdateSpinGameRequest
            {
                GameId = 0,
                BusinessId = 1,
                GameName = "Test Spin Game",
                Description = "A game for testing",
                CreatedByAdminId = 1,
                IsActive = true,
                Configs = new List<SpinGameConfigRequest>
                {
                    new SpinGameConfigRequest
                    {
                        ConfigId = 0,
                        MaxSpinsPerDay = 1,
                        NumberOfSections = 3,
                        GameStartDate = DateTime.Now,
                        GameEndDate = DateTime.Now.AddDays(7),
                        IsActive = true
                    }
                },
                Sections = new List<SpinSectionRequest>
                {
                    new SpinSectionRequest { SectionId = 0, SectionNumber = 1, PrizeText = "Win 10 Points", Points = 10, Color = "#FF0000" }
                }
            };

            // Mock Dapper calls - Game and Config add succeed, Section add fails
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGame", new BaseResponse { ResultId = 1, ResultMessage = "Game added." });
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinGameConfig", new BaseResponse { ResultId = 101, ResultMessage = "Config added." });
            SetupExecuteAsync("UPDATE SpinGame SET ConfigId", 1); // For updating ConfigId in SpinGame
            SetupQueryFirstOrDefaultAsyncForBaseResponse("sp_AddSpinSection", new BaseResponse { ResultId = 0, ResultMessage = "Failed to add/update SpinSection 1." }); // Corrected message

            // Act
            var result = await _repository.AddUpdateSpinGameAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ResultId > 0);
            Assert.Equal("Failed to add/update SpinSection 1.", result.ResultMessage); // Assuming section 1 fails

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
            _mockTransaction.Verify(t => t.Commit(), Times.Never);
            _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        }
    }
}
