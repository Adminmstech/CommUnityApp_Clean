// Track prize image states so edit mode can re-use existing uploads
const uploadedImages = {
    primary: false,
    secondary: false,
    consolation: false
};

// Global edit mode flag
let isEditMode = false;

// Global variable to track the saved game ID (more reliable than DOM)
let savedBrandGameId = 0;

// Update upload status indicator
function updateUploadStatus(prizeType, uploaded, isExisting = false) {
    const input = $(`input[data-prize-type="${prizeType}"]`);
    let statusEl = input.siblings('.upload-status');

    if (statusEl.length === 0) {
        statusEl = $('<span class="upload-status"></span>');
        input.after(statusEl);
    }

    if (uploaded) {
        const text = isExisting ? '✓ Existing' : '✓ Uploaded';
        statusEl.removeClass('pending').addClass('uploaded').text(text);
    } else {
        statusEl.removeClass('uploaded').addClass('pending').text('⚠ Pending');
    }
}

// Check if all images are uploaded and update launch button
function checkAllImagesUploaded() {
    const allUploaded = Object.values(uploadedImages).every(status => status === true);
    const launchBtn = $('#launch-btn');
    const launchText = $('#launch-text');

    if (allUploaded) {
        launchBtn.removeClass('btn-primary').addClass('btn-success activating');
        if (isEditMode) {
            launchText.html('<i class="fa-solid fa-check-circle me-2"></i>Ready to Update');
        } else {
            launchText.html('<i class="fa-solid fa-check-circle me-2"></i>Ready to Launch & Activate');
        }
    } else {
        launchBtn.removeClass('btn-success activating').addClass('btn-primary');
        if (isEditMode) {
            launchText.html('<i class="fa-solid fa-floppy-disk me-2"></i>Update Brand Game');
        } else {
            launchText.html('<i class="fa-solid fa-rocket me-2"></i>Launch Brand Game');
        }
    }
}

function setUploadState(prizeType, uploaded, isExisting = false) {
    uploadedImages[prizeType] = uploaded;
    updateUploadStatus(prizeType, uploaded, isExisting);
}

function initializeUploadStatus(isEditMode) {
    Object.keys(uploadedImages).forEach(prizeType => {
        if (isEditMode) {
            setUploadState(prizeType, true, true);
        } else {
            setUploadState(prizeType, false, false);
        }
    });
    checkAllImagesUploaded();
}

function formatDateForInput(rawValue) {
    if (!rawValue) return '';
    const date = new Date(rawValue);
    if (Number.isNaN(date.getTime())) return '';
    return date.toISOString().split('T')[0];
}

function bindGameForm(data) {
    console.log('Binding form with data:', data);
    console.log('Raw JSON keys:', Object.keys(data));
    
    // Helper to get property value (handles both camelCase and PascalCase)
    function getProp(obj, camelName) {
        const pascalName = camelName.charAt(0).toUpperCase() + camelName.slice(1);
        return obj[camelName] !== undefined ? obj[camelName] : obj[pascalName];
    }
    
    // CRITICAL: Always ensure BrandGameID is set first
    const gameId = getProp(data, 'brandGameID');
    if (gameId && gameId > 0) {
        $('[name="BrandGameID"]').val(gameId);
        console.log('Set BrandGameID to:', gameId);
    }
    
    // Use name attribute selectors since asp-for generates both id and name
    const setters = [
        ['[name="BrandGameName"]', getProp(data, 'brandGameName')],
        ['[name="BrandGameTitle"]', getProp(data, 'brandGameTitle')],
        ['[name="BrandGameDesc"]', getProp(data, 'brandGameDesc')],
        ['[name="DestinationUrl"]', getProp(data, 'destinationUrl')],
        ['[name="PromotionalCode"]', getProp(data, 'promotionalCode')],
        ['[name="OnceIn"]', getProp(data, 'onceIn')],
        ['[name="IsReleased"]', getProp(data, 'isReleased')],
        ['[name="IsPrizeClosed"]', getProp(data, 'isPrizeClosed')],
        ['[name="PrimaryOfferText"]', getProp(data, 'primaryOfferText')],
        ['[name="PrimaryPrizeCount"]', getProp(data, 'primaryPrizeCount')],
        ['[name="PrimaryWinMessage"]', getProp(data, 'primaryWinMessage')],
        ['[name="PrimaryPrizePromotionId"]', getProp(data, 'primaryPrizePromotionId')],
        ['[name="OfferText"]', getProp(data, 'offerText')],
        ['[name="SecondaryPrizeCount"]', getProp(data, 'secondaryPrizeCount')],
        ['[name="SecondaryWinMessage"]', getProp(data, 'secondaryWinMessage')],
        ['[name="ConsolationPrizeCount"]', getProp(data, 'consolationPrizeCount')],
        ['[name="PointsAwarded"]', getProp(data, 'pointsAwarded')],
        ['[name="ConsolationMessage"]', getProp(data, 'consolationMessage')],
        ['[name="PanelCount"]', getProp(data, 'panelCount')],
        ['[name="PanelOpeningLimit"]', getProp(data, 'panelOpeningLimit')]
    ];

    setters.forEach(([selector, value]) => {
        const el = $(selector);
        console.log(`Setting ${selector}:`, value, 'Element found:', el.length > 0);
        if (value !== null && value !== undefined && el.length) {
            el.val(value);
        }
    });

    const dateStartEl = $('[name="DateStart"]');
    if (dateStartEl.length) {
        dateStartEl.val(formatDateForInput(getProp(data, 'dateStart')));
    }
    const dateEndEl = $('[name="DateEnd"]');
    if (dateEndEl.length) {
        dateEndEl.val(formatDateForInput(getProp(data, 'dateEnd')));
    }

    // Show main game image preview if available
    const brandGameImage = getProp(data, 'brandGameImage');
    if (brandGameImage) {
        const mainPreview = $('#main-image-preview');
        mainPreview.find('.preview-img').attr('src', '/' + brandGameImage);
        mainPreview.show();
    }

    // Re-trigger dynamic feedback now that values are bound
    $('[name="IsReleased"]').trigger('change');
    $('[name="IsPrizeClosed"]').trigger('change');
    $('[name="OnceIn"]').trigger('change');
}

function loadGameForEdit(brandGameId) {
    if (!brandGameId) return;

    const detailsUrl = `/Business/Game/Details/${brandGameId}`;
    console.log('Loading game for edit, URL:', detailsUrl);
    
    $.getJSON(detailsUrl)
        .done(function (response) {
            console.log('Game data received:', response);
            bindGameForm(response);
            initializeUploadStatus(true);
            // Handle both camelCase and PascalCase
            const gameId = response.brandGameID !== undefined ? response.brandGameID : response.BrandGameID;
            $('[name="BrandGameID"]').val(gameId);
            // Also update the global tracking variable
            savedBrandGameId = gameId;
            console.log('loadGameForEdit: savedBrandGameId set to', savedBrandGameId);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.error('Failed to load game details:', textStatus, errorThrown, jqXHR.responseText);
        });
}

$(document).ready(function () {
    console.log('Brand game JS loaded');
    
    const brandGameIdFromModel = parseInt($('[name="BrandGameID"]').val() || '0', 10);
    const queryParamId = parseInt(new URLSearchParams(window.location.search).get('brandGameId') || '0', 10);
    const brandGameId = brandGameIdFromModel || queryParamId || 0;
    isEditMode = brandGameId > 0;
    
    // Initialize the global savedBrandGameId to prevent duplicate game creation
    savedBrandGameId = brandGameId;
    console.log('Document ready: savedBrandGameId initialized to', savedBrandGameId);

    console.log('Edit mode detection:', {
        brandGameIdFromModel,
        queryParamId,
        brandGameId,
        isEditMode,
        hiddenInputExists: $('[name="BrandGameID"]').length > 0
    });

    if (brandGameId && brandGameId !== brandGameIdFromModel) {
        $('[name="BrandGameID"]').val(brandGameId);
    }

    // Initialize upload states (existing uploads assumed present in edit mode)
    initializeUploadStatus(isEditMode);

    if (isEditMode) {
        loadGameForEdit(brandGameId);
    }

    // Image preview functionality
    $('.prize-image-upload').on('change', function () {
        const prizeType = $(this).data('prize-type');
        const file = this.files[0];

        if (file) {
            // Validate file size (5MB limit)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                $(this).val('');
                return;
            }

            // Validate file type
            if (!file.type.startsWith('image/')) {
                alert('Please select a valid image file');
                $(this).val('');
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                const preview = $(`#${prizeType}-preview`);
                preview.find('.preview-img').attr('src', e.target.result);
                preview.show();

                // Update upload status
                uploadedImages[prizeType] = true;
                updateUploadStatus(prizeType, true);
                checkAllImagesUploaded();
            };
            reader.readAsDataURL(file);
        } else {
            $(`#${prizeType}-preview`).hide();
            uploadedImages[prizeType] = false;
            updateUploadStatus(prizeType, false);
            checkAllImagesUploaded();
        }
    });

    // Add dynamic interaction for OnceIn frequency
    $('#OnceIn').on('change', function () {
        const value = parseInt($(this).val());
        const helpText = $(this).siblings('.form-text');

        if (value <= 0) {
            $(this).val(1);
            helpText.text('Invalid value. Reset to 1 in 1 (every attempt wins)');
            helpText.css('color', '#ff4d4d');
            setTimeout(() => {
                helpText.css('color', '').text('How often should prizes be awarded (1 in X attempts)');
            }, 2000);
        } else if (value === 1) {
            helpText.text('Every player will win a prize!').css('color', '#28a745');
        } else if (value <= 5) {
            helpText.text(`High win rate: ${Math.round(100 / value)}% chance`).css('color', '#ffc107');
        } else {
            helpText.text(`${Math.round(100 / value)}% win probability`).css('color', '');
        }
    });

    // Dynamic feedback for release status
    $('#IsReleased').on('change', function () {
        const isReleased = $(this).val() === '1';
        const statusIndicator = $(this).closest('.col-md-4').find('.status-indicator');

        if (statusIndicator.length === 0) {
            const indicator = $('<div class="status-indicator mt-2 p-2 rounded"></div>');
            $(this).closest('.col-md-4').append(indicator);
        }

        const indicator = $(this).closest('.col-md-4').find('.status-indicator');

        if (isReleased) {
            indicator.removeClass('bg-warning').addClass('bg-success text-white')
                .html('<i class="fa-solid fa-check-circle me-2"></i>Game will be live and accessible to players');
        } else {
            indicator.removeClass('bg-success text-white').addClass('bg-warning')
                .html('<i class="fa-solid fa-lock me-2"></i>Game will be saved as draft only');
        }
    });

    // Dynamic feedback for prize closed status
    $('#IsPrizeClosed').on('change', function () {
        const isClosed = $(this).val() === '1';
        const statusIndicator = $(this).closest('.col-md-4').find('.prize-status-indicator');

        if (statusIndicator.length === 0) {
            const indicator = $('<div class="prize-status-indicator mt-2 p-2 rounded"></div>');
            $(this).closest('.col-md-4').append(indicator);
        }

        const indicator = $(this).closest('.col-md-4').find('.prize-status-indicator');

        if (isClosed) {
            indicator.removeClass('bg-info').addClass('bg-danger text-white')
                .html('<i class="fa-solid fa-times-circle me-2"></i>Players can play but won\'t receive prizes');
        } else {
            indicator.removeClass('bg-danger text-white').addClass('bg-info')
                .html('<i class="fa-solid fa-gift me-2"></i>Players can win prizes normally');
        }
    });

    // Initializing manual jQuery validation with rules
    $('.game-form').validate({
        rules: {
            BrandGameName: { required: true },
            BrandGameTitle: { required: true },
            BrandGameDesc: { required: true },
            DateStart: { required: true },
            DateEnd: { required: true },

            DestinationUrl: {  url: true },
            //PromotionalCode: { required: true },
            OnceIn: { required: true, min: 1, max: 1000 },
            PrimaryOfferText: { required: true },
            PrimaryPrizeCount: { required: true, min: 0 },
            PrimaryWinMessage: { required: true },
            PrimaryPrizePromotionId: { required: true },
            OfferText: { required: true },
            SecondaryPrizeCount: { required: true, min: 0 },
            SecondaryWinMessage: { required: true },
            ConsolationPrizeCount: { required: true, min: 0 },
            PointsAwarded: { required: true, min: 0 },
            ConsolationMessage: { required: true }
        },
        messages: {
            BrandGameName: "Game name is required",
            BrandGameTitle: "Subtitle is required",
            BrandGameDesc: "Description is required",
            DateStart: "Start date is required",
            DateEnd: "End date is required",

            DestinationUrl: {
                required: "Destination URL is required",
                url: "Please enter a valid URL"
            },
            PromotionalCode: "Required",
            OnceIn: {
                required: "Prize frequency is required",
                min: "Minimum value is 1",
                max: "Maximum value is 1000"
            },
            PrimaryOfferText: "Required",
            PrimaryPrizeCount: "Required",
            PrimaryWinMessage: "Required",
            PrimaryPrizePromotionId: "Required",
            OfferText: "Required",
            SecondaryPrizeCount: "Required",
            SecondaryWinMessage: "Required",
            ConsolationPrizeCount: "Required",
            PointsAwarded: "Required",
            ConsolationMessage: "Required"
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            error.addClass('error');
            error.insertAfter(element);
        },
        highlight: function (element) {
            $(element).addClass('error');
        },
        unhighlight: function (element) {
            $(element).removeClass('error');
        },
        submitHandler: function (form) {
            // Check if all images are uploaded before submitting unless editing (existing uploads allowed)
            const allUploaded = Object.values(uploadedImages).every(status => status === true);

            if (!allUploaded && !isEditMode) {
                alert('Please upload images for all prize types before launching the game.');
                return false;
            }

            // CRITICAL: Verify BrandGameID is set correctly before submitting
            const currentBrandGameId = parseInt($('[name="BrandGameID"]').val() || '0', 10);
            console.log('Submitting form with BrandGameID:', currentBrandGameId, 'isEditMode:', isEditMode);
            
            // If we're in edit mode but BrandGameID is 0, try to recover from query param
            if (isEditMode && currentBrandGameId === 0) {
                const queryParamId = parseInt(new URLSearchParams(window.location.search).get('brandGameId') || '0', 10);
                if (queryParamId > 0) {
                    $('[name="BrandGameID"]').val(queryParamId);
                    console.log('Recovered BrandGameID from query param:', queryParamId);
                }
            }

            // Show activation progress
            const launchBtn = $('#launch-btn');
            const launchText = $('#launch-text');

            launchBtn.prop('disabled', true);
            launchText.html('<i class="fa-solid fa-spinner fa-spin me-2"></i>Activating Game...');

            // Submit the form
            form.submit();
        }
    });

    $('#DateEnd').on('change', function () {
        var startVal = $('#DateStart').val();
        if (startVal) {
            var start = new Date(startVal);
            var end = new Date($(this).val());
            if (end < start) {
                alert('End date cannot be before start date');
                $(this).val(startVal);
            }
        }
    });

    // Initialize dynamic feedback on page load
    $('#IsReleased').trigger('change');
    $('#IsPrizeClosed').trigger('change');
    $('#OnceIn').trigger('change');
});

// Remove image preview function
function removeImagePreview(prizeType) {
    $(`#${prizeType}-preview`).hide();
    $(`input[data-prize-type="${prizeType}"]`).val('');

    // Update global tracking
    if (typeof uploadedImages !== 'undefined') {
        uploadedImages[prizeType] = false;
        setUploadState(prizeType, false, false);
        checkAllImagesUploaded();
    }
}

function goToTab(nextTabId) {
    var $form = $('.game-form');
    var $currentTabPane = $('.tab-pane.show.active');

    // Validate only the inputs within the current tab
    var isValid = true;
    $currentTabPane.find('input, textarea, select').each(function () {
        if ($(this).attr('name')) {
            if (!$form.validate().element(this)) {
                isValid = false;
            }
        }
    });

    if (!isValid) return;

    // Save progress via AJAX
    var formData = new FormData($form[0]);
    
    // CRITICAL: Ensure BrandGameID is set correctly to prevent duplicate game creation
    // Use the global savedBrandGameId if available, otherwise try to get from hidden field or URL
    var currentId = savedBrandGameId || parseInt($('input[name="BrandGameID"]').val() || '0', 10);
    if (currentId === 0) {
        // Try to recover from URL query param
        var queryParamId = parseInt(new URLSearchParams(window.location.search).get('brandGameId') || '0', 10);
        if (queryParamId > 0) {
            currentId = queryParamId;
        }
    }
    
    // Explicitly set/override the BrandGameID in FormData to ensure it's sent correctly
    formData.set('BrandGameID', currentId);
    console.log('goToTab: Sending BrandGameID =', currentId);
    
    var $nextBtn = $currentTabPane.find('.btn-next');
    var originalHtml = $nextBtn.html();
    $nextBtn.prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin me-1"></i> Saving...');

    $.ajax({
        url: '/Business/Game/AjaxSave',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            $nextBtn.prop('disabled', false).html(originalHtml);
            if (response.success) {
                if (response.brandGameId && response.brandGameId > 0) {
                    // Update both the global variable and the hidden field
                    savedBrandGameId = response.brandGameId;
                    $('input[name="BrandGameID"]').val(response.brandGameId);
                    console.log('goToTab: Saved BrandGameID updated to', savedBrandGameId);
                }

                var $nextTrigger = $('[data-bs-target="#' + nextTabId + '"]');
                if ($nextTrigger.length) {
                    bootstrap.Tab.getOrCreateInstance($nextTrigger[0]).show();
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            } else {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'error', title: 'Error', text: response.message });
                } else {
                    alert('Error: ' + response.message);
                }
            }
        },
        error: function () {
            $nextBtn.prop('disabled', false).html(originalHtml);
            alert('Save failed.');
        }
    });
}