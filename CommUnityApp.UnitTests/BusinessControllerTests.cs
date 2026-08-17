using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CommUnityApp.UnitTests
{
    public class BusinessControllerTests
    {
        private readonly Mock<IBusinessRepository> _business = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IEmailService> _email = new();
        private readonly BusinessController _controller;

        public BusinessControllerTests()
        {
            _unitOfWork.SetupGet(unit => unit.Business).Returns(_business.Object);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BaseUrl"] = "https://server.sportzprosys.com.au"
                })
                .Build();

            _controller = new BusinessController(
                Mock.Of<ILogger<BusinessController>>(),
                _unitOfWork.Object,
                config,
                Mock.Of<IHubContext<AuctionHub>>(),
                _email.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Request =
                        {
                            Scheme = "https",
                            Host = new HostString("server.sportzprosys.com.au")
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task AddBusiness_UsesRealisticPayloadSendsCredentialsOnceAndDoesNotDuplicate()
        {
            var request = RealisticBusiness();

            _business
                .Setup(repo => repo.AddBusinessAsync(It.Is<AddBusinessRequest>(model =>
                    model.BusinessId == 0 &&
                    model.BusinessName == "Harbour Spice Grocer" &&
                    model.Email == "owner@harbourspice.example")))
                .ReturnsAsync(new BusinessAddResponse
                {
                    ResultId = 41,
                    ResultMessage = "Business saved successfully.",
                    GeneratedPassword = "Welcome#2026"
                });

            _business
                .Setup(repo => repo.AddBusinessAsync(It.Is<AddBusinessRequest>(model =>
                    model.BusinessName == "Duplicate Harbour Spice Grocer")))
                .ReturnsAsync(new BusinessAddResponse
                {
                    ResultId = 0,
                    ResultMessage = "Business already exists."
                });

            var createdResult = Assert.IsType<OkObjectResult>(await _controller.AddBusiness(request));
            Assert.Equal(41, Assert.IsType<BusinessAddResponse>(createdResult.Value).ResultId);

            _email.Verify(service => service.SendBusinessUserCredentialsEmailAsync(
                "owner@harbourspice.example",
                "Welcome#2026"), Times.Once);

            var duplicateResult = Assert.IsType<OkObjectResult>(await _controller.AddBusiness(new AddBusinessRequest
            {
                BusinessName = "Duplicate Harbour Spice Grocer",
                Email = "owner@harbourspice.example",
                OwnerName = "Meera Shah",
                Phone = "+61 400 111 222",
                Address = "12 Market Lane",
                City = "Sydney",
                State = "NSW",
                Country = "Australia",
                CategoryId = 2
            }));
            Assert.Equal(0, Assert.IsType<BusinessAddResponse>(duplicateResult.Value).ResultId);

            _email.Verify(service => service.SendBusinessUserCredentialsEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);

            _controller.ModelState.AddModelError("Email", "Email is required.");
            Assert.IsType<BadRequestObjectResult>(await _controller.AddBusiness(new AddBusinessRequest()));
        }

        [Fact]
        public async Task BusinessLookupFavouritePostAndLoginApis_ReturnExpectedData()
        {
            var userId = Guid.Parse("11111111-2222-3333-4444-555555555555");

            _business.Setup(repo => repo.GetAllBusinesses(userId)).ReturnsAsync(new List<BusinessDetailsDto>
            {
                new()
                {
                    BusinessId = 41,
                    BusinessName = "Harbour Spice Grocer",
                    CategoryId = 2,
                    CategoryName = "Grocery",
                    BusinessEmail = "owner@harbourspice.example",
                    City = "Sydney",
                    IsFavorite = true,
                    IsVerified = true,
                    IsActive = true
                }
            });
            _business.Setup(repo => repo.GetBusinessDetails(41)).ReturnsAsync(new BusinessDetailsDto
            {
                BusinessId = 41,
                BusinessName = "Harbour Spice Grocer",
                BusinessEmail = "owner@harbourspice.example",
                Phone = "+61 400 111 222"
            });
            _business.Setup(repo => repo.GetBusinessCustomers(41)).ReturnsAsync(new List<CustomerModel>
            {
                new()
                {
                    UserId = userId,
                    Name = "Asha Kumar",
                    Email = "asha@example.com",
                    Mobile = "+61 400 555 111",
                    City = "Parramatta",
                    TotalOrders = 3,
                    TotalSpent = 148.75m,
                    LastOrderDate = new DateTime(2026, 8, 1)
                }
            });
            _business.Setup(repo => repo.GetBusinesscategory()).ReturnsAsync(new List<Category>
            {
                new() { CategoryId = 2, CategoryName = "Grocery", Description = "Food and essentials.", IsActive = true }
            });
            _business.Setup(repo => repo.AddRemoveFavouriteBusiness(41, userId))
                .ReturnsAsync(new BaseResponse { ResultId = 1, ResultMessage = "Business added to favourites." });
            _business.Setup(repo => repo.GetFavouriteBusinesses(userId)).ReturnsAsync(new List<FavouriteBusinessModel>
            {
                new() { FavouriteId = 9, BusinessId = 41, BusinessName = "Harbour Spice Grocer", Logo = "BusinessLogos/harbour.jpg", IsActive = true }
            });
            _business.Setup(repo => repo.GetTopFiveBusinessPosts()).ReturnsAsync(new List<BusinessPostEntity>
            {
                new() { PostId = 501, BusinessId = 41, BusinessName = "Harbour Spice Grocer", Title = "Weekend Thali Boxes", Message = "Fresh preorder boxes for families.", IsActive = true }
            });
            _business.Setup(repo => repo.GetBusinessPostDetails(501)).ReturnsAsync(new BusinessPostDetailsEntity
            {
                PostId = 501,
                BusinessId = 41,
                BusinessName = "Harbour Spice Grocer",
                Title = "Weekend Thali Boxes",
                Message = "Fresh preorder boxes for families.",
                ImagePath = "Uploads/Posts/41/thali.jpg",
                IsActive = true
            });
            _business.Setup(repo => repo.GetAllBusinessPosts(41)).ReturnsAsync(new List<BusinessPostListEntity>
            {
                new() { PostId = 501, BusinessId = 41, BusinessName = "Harbour Spice Grocer", Title = "Weekend Thali Boxes", Message = "Fresh preorder boxes for families.", IsActive = true }
            });
            _business.Setup(repo => repo.BusinessLogin(It.Is<AppBusinessLoginRequest>(model =>
                    model.Email == "owner@harbourspice.example" && model.Password == "Welcome#2026")))
                .ReturnsAsync(new AppBusinessLoginResponse
                {
                    ResultId = 1,
                    ResultMessage = "Login successful.",
                    Status = true,
                    BusinessId = 41,
                    BusinessName = "Harbour Spice Grocer",
                    Email = "owner@harbourspice.example",
                    Logo = "BusinessLogos/harbour.jpg"
                });

            Assert.Single(Assert.IsAssignableFrom<List<BusinessDetailsDto>>(Assert.IsType<OkObjectResult>(await _controller.GetBusinesses(userId)).Value));
            Assert.Equal("Harbour Spice Grocer", Assert.IsType<BusinessDetailsDto>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessDetails(41)).Value).BusinessName);
            Assert.Single(Assert.IsAssignableFrom<List<CustomerModel>>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessCustomer(41)).Value));
            Assert.Single(Assert.IsAssignableFrom<List<Category>>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessCategory()).Value));

            var favouriteToggle = Assert.IsType<OkObjectResult>(await _controller.AddRemoveFavouriteBusiness(new FavouriteBusinessRequest { BusinessId = 41, UserId = userId }));
            Assert.Equal("Business added to favourites.", ReadProperty<string>(favouriteToggle.Value!, "ResultMessage"));

            var favourites = Assert.IsType<OkObjectResult>(await _controller.GetFavouriteBusinesses(userId));
            Assert.Single(ReadProperty<List<FavouriteBusinessModel>>(favourites.Value!, "Data"));

            Assert.Single(ReadProperty<List<BusinessPostEntity>>(Assert.IsType<OkObjectResult>(await _controller.GetTopFiveBusinessPosts()).Value!, "Data"));
            Assert.Equal("Weekend Thali Boxes", ReadProperty<BusinessPostDetailsEntity>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessPostDetails(501)).Value!, "Data").Title);
            Assert.Single(ReadProperty<List<BusinessPostListEntity>>(Assert.IsType<OkObjectResult>(await _controller.GetAllBusinessPosts(41)).Value!, "Data"));

            var login = Assert.IsType<OkObjectResult>(await _controller.BusinessLogin(new AppBusinessLoginRequest
            {
                Email = "owner@harbourspice.example",
                Password = "Welcome#2026"
            }));
            var loginData = Assert.IsType<AppBusinessLoginResponse>(login.Value);
            Assert.True(loginData.Status);
            Assert.Equal("https://server.sportzprosys.com.au/BusinessLogos/harbour.jpg", loginData.Logo);

            var invalidLogin = Assert.IsType<OkObjectResult>(await _controller.BusinessLogin(new AppBusinessLoginRequest { Email = " ", Password = "" }));
            Assert.False(ReadProperty<bool>(invalidLogin.Value!, "Status"));
        }

        [Fact]
        public async Task WalletRewardPromotionAndRedemptionApis_ReturnSuccessAndValidateBadInputs()
        {
            var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

            _business.Setup(repo => repo.AllocateBusinessCoins(It.IsAny<AllocateBusinessCoinsRequest>()))
                .ReturnsAsync(new BaseResponse { ResultId = 1, ResultMessage = "Coins allocated successfully." });
            _business.Setup(repo => repo.RewardMemberFromBusiness(It.IsAny<RewardMemberRequest>()))
                .ReturnsAsync(new BaseResponse { ResultId = 1, ResultMessage = "Member rewarded successfully." });
            _business.Setup(repo => repo.AdjustBusinessWallet(It.IsAny<AdjustBusinessWalletRequest>()))
                .ReturnsAsync(new BaseResponse { ResultId = 1, ResultMessage = "Wallet adjusted successfully." });
            _business.Setup(repo => repo.GetBusinessWallet(41)).ReturnsAsync(new BusinessWalletModel
            {
                BusinessWalletId = 14,
                BusinessId = 41,
                AvailableCoins = 2350
            });
            _business.Setup(repo => repo.GetBusinessWalletTransactions(41)).ReturnsAsync(new List<BusinessWalletTransactionModel>
            {
                new() { Id = 701, BusinessWalletId = 14, TransactionType = "ALLOCATE", Coins = 1000, Notes = "Opening campaign allocation." },
                new() { Id = 702, BusinessWalletId = 14, TransactionType = "REWARD", Coins = -75, ReferenceType = "PromotionShare", ReferenceId = 3001, Notes = "Share reward." }
            });
            _business.Setup(repo => repo.GetTransactionTypes()).ReturnsAsync(new List<TransactionTypeModel>
            {
                new() { TransactionTypeId = 1, TypeCode = "ALLOCATE", TypeName = "Allocate", IsCredit = true, IsActive = true },
                new() { TransactionTypeId = 2, TypeCode = "REWARD", TypeName = "Reward", IsCredit = false, IsActive = true }
            });
            _business.Setup(repo => repo.GetRewardsDashboard(41)).ReturnsAsync(new RewardsDashboardModel
            {
                AvailableCoins = 2350,
                TotalAllocatedCoins = 3000,
                TotalRewardCoinsSpent = 650,
                TotalPromotionShares = 12,
                TotalProductShares = 5,
                TotalUsersRewarded = 9
            });
            _business.Setup(repo => repo.GetShareRewardHistory(41)).ReturnsAsync(new List<ShareRewardHistoryModel>
            {
                new() { UserId = userId, UserName = "Asha Kumar", PromotionId = 3001, PromotionName = "Festival Essentials Pack", SharePlatform = "WhatsApp", RewardCoins = 75 }
            });
            _business.Setup(repo => repo.GetBusinessPromotionRedemptions(41)).ReturnsAsync(new List<BusinessPromotionRedemptionModel>
            {
                new()
                {
                    RedemptionId = 801,
                    PromotionId = 3001,
                    PromotionTitle = "Festival Essentials Pack",
                    OfferHeadline = "Save $12 on pantry boxes",
                    ProductName = "Pantry Box",
                    UserId = userId,
                    CustomerName = "Asha Kumar",
                    RedemptionCode = "HSG-2026-801",
                    QRCodeImage = "Uploads/PromotionQR/hsg-801.png",
                    OriginalPrice = 99m,
                    CoinDiscount = 12m,
                    FinalPrice = 87m,
                    RedemptionStatus = "Pending"
                }
            });
            _business.Setup(repo => repo.GetBusinessPromotions(41)).ReturnsAsync(new List<BusinessPromotionModel>
            {
                new()
                {
                    PromotionId = 3001,
                    BusinessId = 41,
                    PromotionTitle = "Festival Essentials Pack",
                    OfferHeadline = "Save $12 on pantry boxes",
                    PromotionalPrice = 87m,
                    ActualPrice = 99m,
                    PromotionImage = "Uploads/Promotions/festival-pack.jpg",
                    QRCodeImage = "Uploads/PromotionQR/hsg-801.png",
                    IsActive = true
                }
            });
            _business.Setup(repo => repo.ValidatePromotionRedemptionCode(41, "HSG-2026-801")).ReturnsAsync(new ValidatePromotionRedemptionResult
            {
                ResultId = 1,
                ResultMessage = "Valid redemption code.",
                Status = true,
                RedemptionId = 801,
                BusinessId = 41,
                PromotionTitle = "Festival Essentials Pack",
                CustomerName = "Asha Kumar",
                RedemptionCode = "HSG-2026-801",
                RedemptionStatus = "Pending"
            });
            _business.Setup(repo => repo.ConfirmPromotionRedemption(It.IsAny<ConfirmPromotionRedemptionRequest>()))
                .ReturnsAsync(new ConfirmPromotionRedemptionResult
                {
                    ResultId = 1,
                    ResultMessage = "Redemption confirmed successfully.",
                    Status = true,
                    RedemptionId = 801
                });

            Assert.True(ReadProperty<bool>(Assert.IsType<OkObjectResult>(await _controller.AllocateBusinessCoins(new AllocateBusinessCoinsRequest { BusinessId = 41, Coins = 500, Notes = "Community festival boost." })).Value!, "Status"));
            Assert.True(ReadProperty<bool>(Assert.IsType<OkObjectResult>(await _controller.RewardMemberFromBusiness(new RewardMemberRequest { BusinessId = 41, UserId = userId, Coins = 75, ReferenceType = "PromotionShare", ReferenceId = 3001, Notes = "Thanks for sharing." })).Value!, "Status"));
            Assert.True(ReadProperty<bool>(Assert.IsType<OkObjectResult>(await _controller.AdjustBusinessWallet(new AdjustBusinessWalletRequest { BusinessId = 41, Coins = 125, TransactionType = "MANUAL_CREDIT", Notes = "Correction." })).Value!, "Status"));

            Assert.Equal(2350, ReadProperty<BusinessWalletModel>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessWallet(41)).Value!, "Data").AvailableCoins);
            Assert.Equal(2, ReadProperty<List<BusinessWalletTransactionModel>>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessWalletTransactions(41)).Value!, "Data").Count);
            Assert.Equal(2, ReadProperty<List<TransactionTypeModel>>(Assert.IsType<OkObjectResult>(await _controller.GetTransactionTypes()).Value!, "Data").Count);
            Assert.Equal(12, Assert.IsType<RewardsDashboardModel>(Assert.IsType<OkObjectResult>(await _controller.GetRewardsDashboard(41)).Value).TotalPromotionShares);
            Assert.Single(Assert.IsAssignableFrom<List<ShareRewardHistoryModel>>(Assert.IsType<OkObjectResult>(await _controller.GetShareRewardHistory(41)).Value));

            var redemptions = Assert.IsType<OkObjectResult>(await _controller.GetBusinessPromotionRedemptions(41));
            Assert.Equal("https://server.sportzprosys.com.au/Uploads/PromotionQR/hsg-801.png", ReadProperty<List<BusinessPromotionRedemptionModel>>(redemptions.Value!, "Data")[0].QRCodeImage);

            var promotions = Assert.IsType<OkObjectResult>(await _controller.GetBusinessPromotions(41));
            var promotion = ReadProperty<List<BusinessPromotionModel>>(promotions.Value!, "Data")[0];
            Assert.Equal("https://server.sportzprosys.com.au/Uploads/Promotions/festival-pack.jpg", promotion.PromotionImage);
            Assert.Equal("https://server.sportzprosys.com.au/Uploads/PromotionQR/hsg-801.png", promotion.QRCodeImage);

            var validCode = Assert.IsType<OkObjectResult>(await _controller.ValidatePromotionRedemptionCode(new ValidatePromotionRedemptionRequest { BusinessId = 41, RedemptionCode = "HSG-2026-801" }));
            Assert.True(Assert.IsType<ValidatePromotionRedemptionResult>(validCode.Value).Status);

            var confirmed = Assert.IsType<OkObjectResult>(await _controller.ConfirmPromotionRedemption(new ConfirmPromotionRedemptionRequest { BusinessId = 41, RedemptionId = 801 }));
            Assert.True(Assert.IsType<ConfirmPromotionRedemptionResult>(confirmed.Value).Status);

            Assert.False(ReadProperty<bool>(Assert.IsType<OkObjectResult>(await _controller.GetBusinessPromotions(0)).Value!, "Status"));
            Assert.IsType<BadRequestObjectResult>(await _controller.ValidatePromotionRedemptionCode(new ValidatePromotionRedemptionRequest { BusinessId = 0, RedemptionCode = "HSG-2026-801" }));
            Assert.False(ReadProperty<bool>(Assert.IsType<OkObjectResult>(await _controller.ConfirmPromotionRedemption(new ConfirmPromotionRedemptionRequest { BusinessId = 0, RedemptionId = 0 })).Value!, "Status"));
        }

        private static AddBusinessRequest RealisticBusiness()
        {
            return new AddBusinessRequest
            {
                BusinessId = 0,
                CategoryId = 2,
                BusinessName = "Harbour Spice Grocer",
                BusinessNumber = "ABN-53600111222",
                OwnerName = "Meera Shah",
                Email = "owner@harbourspice.example",
                Phone = "+61 400 111 222",
                Address = "12 Market Lane",
                City = "Sydney",
                State = "NSW",
                Country = "Australia",
                Suburb = "Parramatta",
                Logo = "BusinessLogos/harbour-spice.jpg",
                Info = "South Asian grocery, tiffin boxes, and community pantry staples.",
                Latitude = -33.8136m,
                Longitude = 151.0034m,
                WebLink = "https://harbourspice.example",
                Password = "Welcome#2026",
                IsVerified = true,
                IsActive = 1
            };
        }

        private static T ReadProperty<T>(object value, string propertyName)
        {
            return (T)value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!;
        }
    }
}
