/* ~/wwwroot/AdminControllerJSs/CategoryIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // CATEGORY-MODAL ELEMENTS START
    const categoryModal = document.getElementById(`categoryModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // CATEGORY-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupCategoryId = document.getElementById(`popupCategoryId`);
    const popupCategoryTitle = document.getElementById(`popupCategoryTitle`);
    const popupCategoryName = document.getElementById(`popupCategoryName`);
    const popupCategoryDescription = document.getElementById(`popupCategoryDescription`);
    const popupCategoryIsActive = document.getElementById(`popupCategoryIsActive`);
    const popupCategoryCreatedDate = document.getElementById(`popupCategoryCreatedDate`);
    const popupCategoryUpdatedDate = document.getElementById(`popupCategoryUpdatedDate`);
    const popupCategoryImage = document.getElementById(`popupCategoryImage`);
    const popupImageIcon = document.getElementById(`popupImageIcon`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButton = document.querySelectorAll(`.btn-details`);

    detailsButton.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const categoryId = button.dataset.categoryId;
            const categoryName = button.dataset.categoryName;
            const categoryDescription = button.dataset.categoryDescription;
            const categoryIsActive = button.dataset.categoryIsActive;
            const categoryCreatedDate = button.dataset.categoryCreatedDate;
            const categoryUpdatedDate = button.dataset.categoryUpdatedDate;
            const categoryImage = button.dataset.categoryImage;
            // BUTTON DATA END

            // POPUP TEXT DATA START 
            // CategoryID:
            popupCategoryId.textContent = categoryId || `-`;
            // CategoryTitle and CategoryName:
            popupCategoryTitle.textContent = categoryName || `-`;
            popupCategoryName.textContent = categoryName || `-`;
            // CategoryDescription:
            popupCategoryDescription.textContent = categoryDescription && categoryDescription.trim() !== `` ? categoryDescription : `-`;
            // CategoryIsActive:
            popupCategoryIsActive.classList.remove(`active`, `deactive`);
            if (categoryIsActive === `True` || categoryIsActive === `true`) {
                popupCategoryIsActive.textContent = `Aktiv`;
                popupCategoryIsActive.classList.remove(`deactive`);
                popupCategoryIsActive.classList.add(`active`);
            }
            else {
                popupCategoryIsActive.textContent = `Deaktiv`;
                popupCategoryIsActive.classList.remove(`active`);
                popupCategoryIsActive.classList.add(`deactive`);
            }
            // CategoryCreatedDate:
            popupCategoryCreatedDate.textContent = categoryCreatedDate || `-`;
            // CategoryUpdatedDate:
            popupCategoryUpdatedDate.textContent = categoryUpdatedDate || `-`;
            // CategoryImage:
            if (categoryImage && categoryImage.trim() !== ``) {
                popupCategoryImage.src = categoryImage;
                popupCategoryImage.classList.add(`visible`);
                popupImageIcon.classList.add(`hidden`);
            }
            else {
                popupCategoryImage.src = ``;
                popupCategoryImage.classList.remove(`visible`);
                popupImageIcon.classList.remove(`hidden`);
            }
            // POPUP TEXT DATA END

            // Popupı açır:
            categoryModal.classList.add(`active`);

            // Popup açıq olanda bodynin scroll edilə bilinməsini bağlayır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        categoryModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsini aktivləşdirir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popupı bağlamaq:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` &&
            categoryModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
