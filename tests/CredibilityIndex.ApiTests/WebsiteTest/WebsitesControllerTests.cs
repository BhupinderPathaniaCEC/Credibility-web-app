// using System;
// using System.Threading.Tasks;
// using Moq;
// using Xunit;
// using FluentAssertions;
// using Microsoft.AspNetCore.Mvc;
// using CredibilityIndex.Api.Controllers;
// using CredibilityIndex.Application.Interfaces;
// using CredibilityIndex.Domain.Entities;

// namespace CredibilityIndex.ApiTests
// {
//     public class WebsitesControllerTests
//     {
//         private readonly Mock<IWebsiteRepository> _mockRepo;
//         private readonly WebsitesController _controller;

//         public WebsitesControllerTests()
//         {
//             _mockRepo = new Mock<IWebsiteRepository>();
//             _controller = new WebsitesController(_mockRepo.Object);
//         }

//         // [Fact]
//         // public async Task GetById_ReturnsNotFound_WhenWebsiteDoesNotExist()
//         // {
//         //     _mockRepo.Setup(r => r.GetWebsiteWithSnapshotByDomainAsync("test.com"))
//         //              .ReturnsAsync((Website)null);

//         //     // var result = await _controller.GetById(99);
//         //     // result.Should().BeOfType<NotFoundResult>();
//         // }

//         [Fact]
//         public async Task GetById_ReturnsWebsiteWithSnapshot_WhenSnapshotPresent()
//         {
//             // arrange
//             var snapshot = new CredibilitySnapshot
//             {
//                 Score = 42,
//                 AvgAccuracy = 1.1,
//                 AvgBiasNeutrality = 2.2,
//                 AvgTransparency = 3.3,
//                 AvgSafetyTrust = 4.4,
//                 RatingCount = 10,
//                 ComputedAt = DateTime.UtcNow
//             };

//             var website = new Website
//             {
//                 Id = 1,
//                 DisplayName = "Test Site",
//                 Domain = "test.com",
//                 Description = "description",
//                 IsActive = true,
//                 Category = new Category { Id = 5, Name = "Cat", Slug = "cat" },
//                 CredibilitySnapshot = snapshot
//             };

//             _mockRepo.Setup(r => r.GetWebsiteWithSnapshotAsync(1))
//                      .ReturnsAsync(website);

//             // act
//             var result = await _controller.GetById(1);

//             // assert
//             result.Should().BeOfType<OkObjectResult>();
//             var ok = result as OkObjectResult;
//             ok.Should().NotBeNull();

//             var expected = new
//             {
//                 website.Id,
//                 website.Name,
//                 website.Domain,
//                 website.Description,
//                 website.IsActive,
//                 Category = new { website.Category.Id, website.Category.Name },
//                 Snapshot = new
//                 {
//                     snapshot.Score,
//                     snapshot.AvgAccuracy,
//                     snapshot.AvgBiasNeutrality,
//                     snapshot.AvgTransparency,
//                     snapshot.AvgSafetyTrust,
//                     snapshot.RatingCount,
//                     snapshot.ComputedAt
//                 }
//             };

//             ok!.Value.Should().BeEquivalentTo(expected);
//         }
//     }
// }