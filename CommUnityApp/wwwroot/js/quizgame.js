$(document).ready(function () {
    const questionsContainer = $('#questionsContainer');
    const btnAddQuestion = $('#btnAddQuestion');

    // Templates
    const questionTemplate = (index) => `
        <div class="question-card card border shadow-sm rounded-4 mb-4 p-4 animate__animated animate__fadeIn" data-index="${index}">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h5 class="fw-700 mb-0">Question #<span class="q-number">${index + 1}</span></h5>
                <button type="button" class="btn btn-sm btn-outline-danger btn-remove-question">
                    <i class="fa-solid fa-trash me-1"></i> Remove Question
                </button>
            </div>
            <div class="row g-3">
                <input type="hidden" name="Questions[${index}].QuestionId" value="0" />
                <div class="col-md-9">
                    <label class="form-label fw-600">Question Text</label>
                    <input name="Questions[${index}].QuestionText" class="form-control" placeholder="Ask something..." required />
                </div>
                <div class="col-md-3">
                    <label class="form-label fw-600">Points</label>
                    <input type="number" name="Questions[${index}].Points" value="10" class="form-control" min="1" />
                </div>
                <div class="col-md-12">
                    <label class="form-label fw-600">Question Image (Optional)</label>
                    <input type="file" name="Questions[${index}].QuestionImageFile" class="form-control" accept="image/*" />
                </div>
            </div>
            <div class="options-container mt-4 ps-4 border-start border-3 border-light">
                <h6 class="fw-600 mb-3"><i class="fa-solid fa-list-ol me-2"></i> Options</h6>
                <div class="options-list">
                    <!-- Default 2 options -->
                    ${optionTemplate(index, 0)}
                    ${optionTemplate(index, 1)}
                </div>
                <button type="button" class="btn btn-sm btn-link text-primary btn-add-option p-0">
                    <i class="fa-solid fa-plus me-1"></i> Add Alternative
                </button>
            </div>
        </div>
    `;

    function optionTemplate(qIndex, oIndex) {
        return `
            <div class="option-row row g-2 mb-2 align-items-center" data-index="${oIndex}">
                <input type="hidden" name="Questions[${qIndex}].Options[${oIndex}].OptionId" value="0" />
                <div class="col-auto">
                    <div class="form-check">
                        <input type="radio" name="Questions[${qIndex}].CorrectRadio" class="form-check-input correct-radio" value="${oIndex}" ${oIndex === 0 ? 'checked' : ''} />
                        <input type="hidden" name="Questions[${qIndex}].Options[${oIndex}].IsCorrect" value="${oIndex === 0 ? 'true' : 'false'}" class="is-correct-hidden" />
                    </div>
                </div>
                <div class="col">
                    <input name="Questions[${qIndex}].Options[${oIndex}].OptionText" class="form-control form-control-sm" placeholder="Option text" required />
                </div>
                <div class="col-auto">
                    <button type="button" class="btn btn-sm btn-link text-danger btn-remove-option">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
            </div>
        `;
    }

    // Add Question
    btnAddQuestion.click(function () {
        const nextIndex = questionsContainer.find('.question-card').length;
        questionsContainer.append(questionTemplate(nextIndex));
    });

    // Remove Question
    questionsContainer.on('click', '.btn-remove-question', function () {
        if (confirm('Are you sure you want to remove this question?')) {
            $(this).closest('.question-card').remove();
            reIndexAll();
        }
    });

    // Add Option
    questionsContainer.on('click', '.btn-add-option', function () {
        const qCard = $(this).closest('.question-card');
        const qIndex = qCard.data('index');
        const oList = qCard.find('.options-list');
        const oIndex = oList.find('.option-row').length;
        oList.append(optionTemplate(qIndex, oIndex));
    });

    // Remove Option
    questionsContainer.on('click', '.btn-remove-option', function () {
        const qCard = $(this).closest('.question-card');
        const oRow = $(this).closest('.option-row');
        if (qCard.find('.option-row').length <= 1) {
            alert('A question must have at least one option.');
            return;
        }
        oRow.remove();
        reIndexQuestionOptions(qCard);
    });

    // Handle Correct Option Radio
    questionsContainer.on('change', '.correct-radio', function () {
        const qCard = $(this).closest('.question-card');
        const clickedIndex = $(this).val();
        
        qCard.find('.is-correct-hidden').val('false');
        qCard.find(`.option-row[data-index="${clickedIndex}"] .is-correct-hidden`).val('true');
    });

    function reIndexAll() {
        questionsContainer.find('.question-card').each(function (qIdx) {
            const qCard = $(this);
            qCard.data('index', qIdx);
            qCard.attr('data-index', qIdx);
            qCard.find('.q-number').text(qIdx + 1);

            qCard.find('input[name^="Questions"]').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    const newName = name.replace(/Questions\[\d+\]/, `Questions[${qIdx}]`);
                    $(this).attr('name', newName);
                }
            });

            reIndexQuestionOptions(qCard);
        });
    }

    function reIndexQuestionOptions(qCard) {
        const qIdx = qCard.data('index');
        qCard.find('.option-row').each(function (oIdx) {
            const oRow = $(this);
            oRow.data('index', oIdx);
            oRow.attr('data-index', oIdx);

            oRow.find('input').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    let newName = name.replace(/Questions\[\d+\]/, `Questions[${qIdx}]`);
                    newName = newName.replace(/Options\[\d+\]/, `Options[${oIdx}]`);
                    $(this).attr('name', newName);
                }
            });

            // Update radio value
            oRow.find('.correct-radio').val(oIdx);
            // Re-sync correct radio name to group it by question
            oRow.find('.correct-radio').attr('name', `Questions[${qIdx}].CorrectRadio`);
        });
    }

    // Form Submission
    $('#quizForm').submit(function (e) {
        e.preventDefault();
        
        // Basic Validation
        if (questionsContainer.find('.question-card').length === 0) {
            alert('Please add at least one question to the quiz.');
            return;
        }

        const formData = new FormData(this);
        const submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin me-2"></i> Saving...');

        $.ajax({
            url: '/Business/QuizGame/AjaxSave',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    alert('Quiz saved successfully!');
                    window.location.href = '/Business/QuizGame/Index';
                } else {
                    alert('Error: ' + response.message);
                    submitBtn.prop('disabled', false).html('<i class="fa-solid fa-rocket me-2"></i> Save Quiz Game');
                }
            },
            error: function () {
                alert('An unexpected error occurred.');
                submitBtn.prop('disabled', false).html('<i class="fa-solid fa-rocket me-2"></i> Save Quiz Game');
            }
        });
    });
});
