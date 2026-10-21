/* ~/wwwroot/AdminControllerJSs/AuthorIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // AUTHOR-MODAL ELEMENTS START
    const authorModal = document.getElementById(`authorModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // AUTHOR-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupAuthorId = document.getElementById(`popupAuthorId`);
    const popupAuthorTitle = document.getElementById(`popupAuthorTitle`);
    const popupAuthorName = document.getElementById(`popupAuthorName`);
    const popupAuthorSurname = document.getElementById(`popupAuthorSurname`);
    const popupAuthorInfo = document.getElementById(`popupAuthorInfo`);
    const popupAuthorIsActive = document.getElementById(`popupAuthorIsActive`);
    const popupAuthorCreatedDate = document.getElementById(`popupAuthorCreatedDate`);
    const popupAuthorUpdatedDate = document.getElementById(`popupAuthorUpdatedDate`);
    const popupAuthorImage = document.getElementById(`popupAuthorImage`);
    const popupImageIcon = document.getElementById(`popupImageIcon`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButton = document.querySelectorAll(`.btn-details`);

    detailsButton.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const authorId = button.dataset.authorId;
            const authorName = button.dataset.authorName;
            const authorSurname = button.dataset.authorSurname;
            const authorInfo = button.dataset.authorInfo;
            const authorIsActive = button.dataset.authorIsActive;
            const authorCreatedDate = button.dataset.authorCreatedDate;
            const authorUpdatedDate = button.dataset.authorUpdatedDate;
            const authorImage = button.dataset.authorImage;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // AuthorID:
            popupAuthorId.textContent = authorId || `-`;
            // AuthorTitle (ad + soyad):
            popupAuthorTitle.textContent = `${authorName || ``} ${authorSurname || ``}`.trim() || `-`;
            // AuthorName:
            popupAuthorName.textContent = authorName || `-`;
            // AuthorSurname:
            popupAuthorSurname.textContent = authorSurname || `-`;
            // AuthorInfo:
            popupAuthorInfo.textContent = authorInfo && authorInfo.trim() !== `` ? authorInfo : `-`;
            // AuthorIsActive:
            popupAuthorIsActive.classList.remove(`active`, `deactive`);
            if (authorIsActive === `True` || authorIsActive === `true`) {
                popupAuthorIsActive.textContent = `Aktiv`;
                popupAuthorIsActive.classList.add(`active`);
            }
            else {
                popupAuthorIsActive.textContent = `Deaktiv`;
                popupAuthorIsActive.classList.add(`deactive`);
            }
            // AuthorCreatedDate:
            popupAuthorCreatedDate.textContent = authorCreatedDate || `-`;
            // AuthorUpdatedDate:
            popupAuthorUpdatedDate.textContent = authorUpdatedDate || `-`;
            // AuthorImage:
            if (authorImage && authorImage.trim() !== ``) {
                popupAuthorImage.src = authorImage;
                popupAuthorImage.classList.add(`visible`);
                popupImageIcon.classList.add(`hidden`);
            }
            else {
                popupAuthorImage.src = ``;
                popupAuthorImage.classList.remove(`visible`);
                popupImageIcon.classList.remove(`hidden`);
            }
            // POPUP TEXT DATA END

            // Popupı açır:
            authorModal.classList.add(`active`);

            // Popup açıq olanda bodynin scroll edilə bilinməsini bağlayır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        authorModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsini aktivləşdirir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popupı bağlamaq:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` && authorModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
