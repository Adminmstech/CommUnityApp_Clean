using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.Areas.Business.Controllers
{
    [Area("Business")]
    public class QuizGameController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QuizGameController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");

            int businessId = int.Parse(businessIdStr);
            var quizzes = await _unitOfWork.QuizGames.GetQuizGamesByBusinessAsync(businessId);
            return View(quizzes);
        }

        public async Task<IActionResult> Create(int? quizId = null)
        {
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            if (string.IsNullOrEmpty(businessIdStr)) return RedirectToAction("Login", "Account");

            var model = new AddUpdateQuizGameRequest
            {
                QuizId = quizId ?? 0,
                BusinessId = int.Parse(businessIdStr),
                Questions = new List<QuizQuestionRequest>()
            };

            if (model.QuizId > 0)
            {
                var quiz = await _unitOfWork.QuizGames.GetQuizGameByIdAsync(model.QuizId);
                if (quiz != null)
                {
                    model.GameName = quiz.GameName;
                    model.Description = quiz.Description;
                    model.QuizImage = quiz.QuizImage;
                    model.ConfigId = quiz.ConfigId;
                    model.IsActive = quiz.IsActive;

                    if (quiz.ConfigId.HasValue)
                    {
                        var config = await _unitOfWork.QuizGames.GetConfigByIdAsync(quiz.ConfigId.Value);
                        if (config != null) model.Config = config;
                    }

                    var questions = await _unitOfWork.QuizGames.GetQuestionsByQuizIdAsync(model.QuizId);
                    foreach (var q in questions)
                    {
                        q.Options = (await _unitOfWork.QuizGames.GetOptionsByQuestionIdAsync(q.QuestionId)).ToList();
                        model.Questions.Add(q);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AjaxSave([FromForm] AddUpdateQuizGameRequest model)
        {
            try
            {
                var businessIdStr = HttpContext.Session.GetString("BusinessId");
                if (string.IsNullOrEmpty(businessIdStr)) return Json(new { success = false, message = "Session expired." });

                model.BusinessId = int.Parse(businessIdStr);
                
                // Handle Quiz Image
                if (model.QuizImageFile != null && model.QuizImageFile.Length > 0)
                {
                    model.QuizImage = await SaveFile(model.QuizImageFile, "quizzes");
                }

                // Handle Question Images
                if (model.Questions != null)
                {
                    foreach (var q in model.Questions)
                    {
                        if (q.QuestionImageFile != null && q.QuestionImageFile.Length > 0)
                        {
                            q.QuestionImage = await SaveFile(q.QuestionImageFile, "quiz_questions");
                        }
                    }
                }

                var response = await _unitOfWork.QuizGames.AddUpdateQuizGameAsync(model);
                if (response.ResultId > 0)
                {
                    return Json(new { success = true, quizId = response.ResultId, message = "Quiz saved successfully." });
                }

                return Json(new { success = false, message = response.ResultMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/images/{folderName}/" + uniqueFileName;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _unitOfWork.QuizGames.DeleteQuizGameAsync(id);
            return Json(new { success = response.ResultId > 0, message = response.ResultMessage });
        }
    }
}
