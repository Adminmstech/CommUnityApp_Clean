using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Hubs;
using CommUnityApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommUnityApp.UnitTests
{
    public class TalentShowControllerTests : IDisposable
    {
        private readonly Mock<ITalentShowRepository> _talentShow = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IClientProxy> _clientProxy = new();
        private readonly Mock<IHubClients> _hubClients = new();
        private readonly Mock<IHubContext<TalentShowHub>> _hubContext = new();
        private readonly string _webRootPath;
        private readonly TalentShowController _controller;

        public TalentShowControllerTests()
        {
            _webRootPath = Path.Combine(Path.GetTempPath(), "communityapp-talent-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_webRootPath);

            _unitOfWork.SetupGet(unit => unit.TalentShow).Returns(_talentShow.Object);
            _hubClients.Setup(clients => clients.Group("TalentShow")).Returns(_clientProxy.Object);
            _hubContext.SetupGet(hub => hub.Clients).Returns(_hubClients.Object);

            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(env => env.WebRootPath).Returns(_webRootPath);

            _controller = new TalentShowController(
                Mock.Of<ILogger<TalentShowController>>(),
                _unitOfWork.Object,
                environment.Object,
                _hubContext.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task CampaignApis_UseRealisticDataAndPreventDuplicateNames()
        {
            var campaigns = new List<TalentShowCampaign>
            {
                TalentCampaign(10, "Christmas Talent Fest 2026"),
                TalentCampaign(11, "New Year Community Showcase 2026")
            };
            campaigns[1].UploadStartDate = DateTime.UtcNow.AddDays(-35);
            campaigns[1].UploadEndDate = DateTime.UtcNow.AddDays(-5);
            campaigns[1].IsUploadOpen = false;

            _talentShow
                .Setup(repo => repo.GetCampaignsAsync(false, true))
                .ReturnsAsync(campaigns.Where(campaign => campaign.IsUploadOpen).ToList());
            _talentShow.Setup(repo => repo.GetCampaignByIdAsync(10)).ReturnsAsync(campaigns[0]);
            _talentShow.Setup(repo => repo.GetCampaignByIdAsync(999)).ReturnsAsync((TalentShowCampaign?)null);
            _talentShow
                .Setup(repo => repo.SaveCampaignAsync(It.Is<TalentShowCampaign>(campaign =>
                    campaign.CampaignName == "Christmas Talent Fest 2026")))
                .ReturnsAsync(new BaseResponse { ResultId = 0, ResultMessage = "Campaign name already exists." });
            _talentShow
                .Setup(repo => repo.SaveCampaignAsync(It.Is<TalentShowCampaign>(campaign =>
                    campaign.CampaignName == "Autumn Arts Showcase 2026")))
                .ReturnsAsync(new BaseResponse { ResultId = 12, ResultMessage = "Campaign saved successfully." });

            var listResult = Assert.IsType<OkObjectResult>(await _controller.GetCampaigns(uploadOpenOnly: true));
            var list = Assert.IsAssignableFrom<List<TalentShowCampaign>>(listResult.Value);
            Assert.Single(list);
            Assert.Equal("Christmas Talent Fest 2026", list[0].CampaignName);

            var detailResult = Assert.IsType<OkObjectResult>(await _controller.GetCampaign(10));
            Assert.Equal("Christmas Talent Fest 2026", Assert.IsType<TalentShowCampaign>(detailResult.Value).CampaignName);

            var missingResult = Assert.IsType<NotFoundObjectResult>(await _controller.GetCampaign(999));
            Assert.Equal("Campaign not found", ReadProperty<string>(missingResult.Value!, "ResultMessage"));

            var duplicateResult = Assert.IsType<OkObjectResult>(await _controller.SaveCampaign(TalentCampaign(0, "Christmas Talent Fest 2026")));
            Assert.Equal(0, Assert.IsType<BaseResponse>(duplicateResult.Value).ResultId);

            var savedResult = Assert.IsType<OkObjectResult>(await _controller.SaveCampaign(TalentCampaign(0, "Autumn Arts Showcase 2026")));
            Assert.Equal(12, Assert.IsType<BaseResponse>(savedResult.Value).ResultId);
        }

        [Fact]
        public async Task CategoryApis_ListLookupSaveAndValidateRequiredName()
        {
            var categories = new List<TalentShowCategory>
            {
                new() { TalentShowCategoryId = 5, CategoryName = "Singing", Description = "Solo and group vocals.", IsActive = true },
                new() { TalentShowCategoryId = 6, CategoryName = "Classical Dance", Description = "Traditional performance.", IsActive = true }
            };

            _talentShow.Setup(repo => repo.GetCategoriesAsync(false)).ReturnsAsync(categories);
            _talentShow.Setup(repo => repo.GetCategoryByIdAsync(5)).ReturnsAsync(categories[0]);
            _talentShow.Setup(repo => repo.GetCategoryByIdAsync(404)).ReturnsAsync((TalentShowCategory?)null);
            _talentShow
                .Setup(repo => repo.SaveCategoryAsync(It.Is<TalentShowCategory>(category => category.CategoryName == "Instrumental")))
                .ReturnsAsync(new BaseResponse { ResultId = 7, ResultMessage = "Category saved successfully." });

            var listResult = Assert.IsType<OkObjectResult>(await _controller.GetCategories());
            Assert.Equal(2, Assert.IsAssignableFrom<List<TalentShowCategory>>(listResult.Value).Count);

            var detailResult = Assert.IsType<OkObjectResult>(await _controller.GetCategory(5));
            Assert.Equal("Singing", Assert.IsType<TalentShowCategory>(detailResult.Value).CategoryName);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetCategory(404));
            Assert.IsType<BadRequestObjectResult>(await _controller.SaveCategory(new TalentShowCategory { CategoryName = " " }));

            var saveResult = Assert.IsType<OkObjectResult>(await _controller.SaveCategory(new TalentShowCategory { CategoryName = "Instrumental" }));
            Assert.Equal(7, Assert.IsType<BaseResponse>(saveResult.Value).ResultId);
        }

        [Fact]
        public async Task RegistrationApis_SaveAndReturnChallengeStatus()
        {
            var request = new TalentShowRegistrationRequest
            {
                TalentShowCampaignId = 10,
                TalentShowCategoryId = 5,
                MemberId = 7001,
                MemberName = "Priya Menon",
                AgeGroup = "13-17",
                AcceptedTerms = true
            };

            _talentShow
                .Setup(repo => repo.RegisterForCampaignAsync(It.Is<TalentShowRegistrationRequest>(model =>
                    model.MemberId == 7001 && model.AgeGroup == "13-17")))
                .ReturnsAsync(new BaseResponse { ResultId = 101, ResultMessage = "Campaign registration saved successfully. Challenge 1 is unlocked." });
            _talentShow
                .Setup(repo => repo.GetRegistrationStatusAsync(10, 7001, "member-7001"))
                .ReturnsAsync(new TalentShowRegistrationStatus
                {
                    TalentShowRegistrationId = 101,
                    TalentShowCampaignId = 10,
                    CampaignName = "Christmas Talent Fest 2026",
                    TalentShowCategoryId = 5,
                    CategoryName = "Singing",
                    MemberId = 7001,
                    MemberName = "Priya Menon",
                    AgeGroup = "13-17",
                    AcceptedTerms = true,
                    CurrentChallengeNumber = 2,
                    CompletedChallenges = 1,
                    Challenges = new List<TalentShowChallengeStatus>
                    {
                        new() { ChallengeNumber = 1, ChallengeLevel = "Beginner", IsUnlocked = true, IsUploaded = true, TalentShowVideoId = 9001 },
                        new() { ChallengeNumber = 2, ChallengeLevel = "Intermediate", IsUnlocked = true, IsUploaded = false },
                        new() { ChallengeNumber = 3, ChallengeLevel = "Advanced / Signature Performance", IsUnlocked = false, IsUploaded = false }
                    }
                });

            var missingCampaignResult = Assert.IsType<BadRequestObjectResult>(await _controller.RegisterForCampaign(new TalentShowRegistrationRequest
            {
                TalentShowCampaignId = 0,
                TalentShowCategoryId = request.TalentShowCategoryId,
                MemberId = request.MemberId,
                MemberName = request.MemberName,
                AgeGroup = request.AgeGroup,
                AcceptedTerms = request.AcceptedTerms
            }));
            Assert.Equal("Campaign is required", ReadProperty<string>(missingCampaignResult.Value!, "ResultMessage"));

            var saveResult = Assert.IsType<OkObjectResult>(await _controller.RegisterForCampaign(request));
            Assert.Equal(101, Assert.IsType<BaseResponse>(saveResult.Value).ResultId);

            var statusResult = Assert.IsType<OkObjectResult>(await _controller.GetRegistrationStatus(10, 7001, "member-7001"));
            var status = Assert.IsType<TalentShowRegistrationStatus>(statusResult.Value);
            Assert.Equal(3, status.Challenges.Count);
            Assert.Equal(2, status.CurrentChallengeNumber);

            Assert.IsType<BadRequestObjectResult>(await _controller.GetRegistrationStatus(0, 7001, "member-7001"));
            Assert.IsType<NotFoundObjectResult>(await _controller.GetRegistrationStatus(10, 9999, "unknown"));
        }

        [Fact]
        public async Task VideoApis_FilterUploadApproveReactSummarizeAndRank()
        {
            var campaign = TalentCampaign(10, "Christmas Talent Fest 2026");
            var category = new TalentShowCategory { TalentShowCategoryId = 5, CategoryName = "Singing", IsActive = true };
            var videos = new List<TalentShowVideo>
            {
                new()
                {
                    TalentShowVideoId = 9001,
                    TalentShowCampaignId = 10,
                    TalentShowCategoryId = 5,
                    CampaignName = campaign.CampaignName,
                    CategoryName = category.CategoryName,
                    MemberId = 7001,
                    MemberName = "Priya Menon",
                    AgeGroup = "13-17",
                    ChallengeNumber = 1,
                    ChallengeLevel = "Beginner",
                    Title = "Carnatic Vocal Round One",
                    Description = "A two-minute alapana and kriti excerpt.",
                    VideoPath = "Uploads/TalentShow/Videos/priya-round-one.mp4",
                    IsApproved = true,
                    ApprovalStatus = "Approved",
                    LikeCount = 18,
                    RatingCount = 6,
                    AverageRating = 4.7m,
                    JudgeScore = 89m,
                    Score = 135.2m
                }
            };

            _talentShow.Setup(repo => repo.GetVideosAsync(It.IsAny<TalentShowVideoFilter>())).ReturnsAsync(videos);
            _talentShow.Setup(repo => repo.GetCampaignByIdAsync(10)).ReturnsAsync(campaign);
            _talentShow.Setup(repo => repo.GetCategoryByIdAsync(5)).ReturnsAsync(category);
            _talentShow.Setup(repo => repo.SaveVideoAsync(It.Is<TalentShowVideo>(video =>
                    video.MemberId == 7002 &&
                    video.Title == "Bollywood Contemporary Round Two" &&
                    video.ChallengeLevel == "Intermediate")))
                .ReturnsAsync(new BaseResponse { ResultId = 9002, ResultMessage = "Video saved successfully." });
            _talentShow.Setup(repo => repo.SaveVideoApprovalAsync(It.IsAny<TalentShowVideoApprovalRequest>()))
                .ReturnsAsync(new BaseResponse { ResultId = 9002, ResultMessage = "Video approved successfully." });
            _talentShow.Setup(repo => repo.SaveReactionAsync(It.IsAny<TalentShowReactionRequest>()))
                .ReturnsAsync(new BaseResponse { ResultId = 9001, ResultMessage = "Reaction saved successfully." });
            _talentShow.Setup(repo => repo.GetVideoSummaryAsync(9001)).ReturnsAsync(new TalentShowVideoSummary
            {
                TalentShowVideoId = 9001,
                LikeCount = 19,
                RatingCount = 7,
                AverageRating = 4.74m,
                JudgeScore = 89m,
                Score = 141.18m
            });
            _talentShow.Setup(repo => repo.GetRankingsAsync(10, 5, "13-17")).ReturnsAsync(new List<TalentShowRanking>
            {
                new()
                {
                    RankNumber = 1,
                    TalentShowCampaignId = 10,
                    CampaignName = campaign.CampaignName,
                    TalentShowCategoryId = 5,
                    CategoryName = "Singing",
                    AgeGroup = "13-17",
                    MemberId = 7001,
                    MemberName = "Priya Menon",
                    JudgeScore = 89m,
                    ViewerRating = 4.74m,
                    EngagementScore = 26m,
                    ConsistencyScore = 33.33m,
                    FinalScore = 67.89m,
                    CompletedChallenges = 1
                }
            });

            var videosResult = Assert.IsType<OkObjectResult>(await _controller.GetVideos(new TalentShowVideoFilter { PageNumber = -4, PageSize = 999 }));
            Assert.Single(Assert.IsAssignableFrom<List<TalentShowVideo>>(videosResult.Value));
            _talentShow.Verify(repo => repo.GetVideosAsync(It.Is<TalentShowVideoFilter>(filter =>
                filter.PageNumber == 1 && filter.PageSize == 24)), Times.Once);

            var uploadRequest = new TalentShowVideoBase64UploadRequest
            {
                TalentShowCampaignId = 10,
                TalentShowCategoryId = 5,
                ChallengeNumber = 2,
                AgeGroup = "13-17",
                MemberId = 7002,
                MemberName = "Aarav Singh",
                Title = "Bollywood Contemporary Round Two",
                Description = "Clean choreography with a clear opening pose.",
                VideoBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("tiny test video content")),
                VideoExtension = ".mp4",
                ThumbnailBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("tiny test thumbnail")),
                ThumbnailExtension = ".jpg"
            };

            var uploadResult = Assert.IsType<OkObjectResult>(await _controller.UploadVideoBase64(uploadRequest));
            Assert.Equal(9002, ReadProperty<int>(uploadResult.Value!, "ResultId"));
            Assert.StartsWith("Uploads/TalentShow/Videos/", ReadProperty<string>(uploadResult.Value!, "VideoPath"));
            _clientProxy.Verify(proxy => proxy.SendCoreAsync(
                "talentShowVideoCreated",
                It.Is<object?[]>(args => Convert.ToInt32(args[0]) == 9002),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsType<BadRequestObjectResult>(await _controller.UploadVideoBase64(new TalentShowVideoBase64UploadRequest
            {
                TalentShowCampaignId = uploadRequest.TalentShowCampaignId,
                TalentShowCategoryId = uploadRequest.TalentShowCategoryId,
                ChallengeNumber = 4,
                AgeGroup = uploadRequest.AgeGroup,
                MemberId = uploadRequest.MemberId,
                MemberName = uploadRequest.MemberName,
                Title = uploadRequest.Title,
                Description = uploadRequest.Description,
                VideoBase64 = uploadRequest.VideoBase64,
                VideoExtension = uploadRequest.VideoExtension,
                ThumbnailBase64 = uploadRequest.ThumbnailBase64,
                ThumbnailExtension = uploadRequest.ThumbnailExtension
            }));
            Assert.IsType<BadRequestObjectResult>(await _controller.SaveVideoApproval(new TalentShowVideoApprovalRequest { TalentShowVideoId = 0, JudgeScore = 50 }));
            Assert.IsType<BadRequestObjectResult>(await _controller.SaveReaction(new TalentShowReactionRequest { TalentShowVideoId = 9001 }));

            var approvalResult = Assert.IsType<OkObjectResult>(await _controller.SaveVideoApproval(new TalentShowVideoApprovalRequest
            {
                TalentShowVideoId = 9002,
                IsApproved = true,
                ApprovalStatus = "Approved",
                JudgeScore = 92,
                JudgeFeedback = "Confident rhythm and stage energy.",
                JudgeSuggestions = "Hold the final pose slightly longer."
            }));
            Assert.Equal(9002, Assert.IsType<BaseResponse>(approvalResult.Value).ResultId);

            var reactionResult = Assert.IsType<OkObjectResult>(await _controller.SaveReaction(new TalentShowReactionRequest
            {
                TalentShowVideoId = 9001,
                MemberId = 8001,
                VisitorKey = "viewer-8001",
                IsLike = true,
                Rating = 5
            }));
            Assert.Equal(9001, ReadProperty<int>(reactionResult.Value!, "ResultId"));
            Assert.NotNull(ReadProperty<object>(reactionResult.Value!, "Summary"));

            var summaryResult = Assert.IsType<OkObjectResult>(await _controller.GetVideoSummary(9001));
            Assert.Equal(19, Assert.IsType<TalentShowVideoSummary>(summaryResult.Value).LikeCount);

            var rankingResult = Assert.IsType<OkObjectResult>(await _controller.GetRankings(10, 5, "13-17"));
            Assert.Single(Assert.IsAssignableFrom<List<TalentShowRanking>>(rankingResult.Value));
        }

        private static TalentShowCampaign TalentCampaign(int id, string name)
        {
            return new TalentShowCampaign
            {
                TalentShowCampaignId = id,
                CampaignName = name,
                EventName = "Community Christmas Concert",
                Description = "Three-stage community talent challenge.",
                Guidelines = "One upload per challenge with original family-friendly content.",
                TermsAndConditions = "Participants agree to public gallery display after moderation.",
                DurationDays = 28,
                ChallengeCount = 3,
                DaysPerChallenge = 7,
                UploadStartDate = DateTime.UtcNow.AddDays(-2),
                UploadEndDate = DateTime.UtcNow.AddDays(26),
                IsActive = true,
                IsUploadOpen = true
            };
        }

        private static T ReadProperty<T>(object value, string propertyName)
        {
            return (T)value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!;
        }

        public void Dispose()
        {
            if (Directory.Exists(_webRootPath))
                Directory.Delete(_webRootPath, recursive: true);
        }
    }
}
