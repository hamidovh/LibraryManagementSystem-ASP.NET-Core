/* ~/wwwroot/AdminControllerJSs/BookIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // BOOK-MODAL ELEMENTS START
    const bookModal = document.getElementById(`bookModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // BOOK-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupBookId = document.getElementById(`popupBookId`);
    const popupBookTitle = document.getElementById(`popupBookTitle`);
    const popupBookName = document.getElementById(`popupBookName`);
    const popupBookIsbn = document.getElementById(`popupBookIsbn`);
    const popupBookAuthors = document.getElementById(`popupBookAuthors`);
    const popupBookCategories = document.getElementById(`popupBookCategories`);
    const popupBookAbout = document.getElementById(`popupBookAbout`);
    const popupBookStockQuantity = document.getElementById(`popupBookStockQuantity`);
    const popupBookFineAmount = document.getElementById(`popupBookFineAmount`);
    const popupBookIsActive = document.getElementById(`popupBookIsActive`);
    const popupBookCreatedDate = document.getElementById(`popupBookCreatedDate`);
    const popupBookUpdatedDate = document.getElementById(`popupBookUpdatedDate`);
    const popupBookImage = document.getElementById(`popupBookImage`);
    const popupImageIcon = document.getElementById(`popupImageIcon`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButton = document.querySelectorAll(`.btn-details`);

    detailsButton.forEach(button => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const bookId = button.dataset.bookId;
            const bookName = button.dataset.bookName;
            const bookIsbn = button.dataset.bookIsbn;
            const bookAbout = button.dataset.bookAbout;
            const bookStockQuantity = button.dataset.bookStockQuantity;
            const bookFineAmount = button.dataset.bookFineAmount;
            const bookIsActive = button.dataset.bookIsActive;
            const bookCreatedDate = button.dataset.bookCreatedDate;
            const bookUpdatedDate = button.dataset.bookUpdatedDate;
            const bookAuthors = button.dataset.bookAuthors;
            const bookCategories = button.dataset.bookCategories;
            const bookImage = button.dataset.bookImage;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // BookID:
            popupBookId.textContent = bookId || `-`;
            // BookTitle and BookName:
            popupBookTitle.textContent = bookName || `Kitab`;
            popupBookName.textContent = bookName || `-`;
            // BookIsbn:
            popupBookIsbn.textContent = bookIsbn || `-`;
            // BookAuthors:
            popupBookAuthors.textContent = bookAuthors || `-`;
            // BookCategories:
            popupBookCategories.textContent = bookCategories || `-`;
            // BookAbout:
            popupBookAbout.textContent = bookAbout || `-`;
            // BookStockQuantity:
            popupBookStockQuantity.textContent = bookStockQuantity || `-`;
            // BookFineAmount:
            popupBookFineAmount.textContent = bookFineAmount ? `${bookFineAmount} AZN` : `-`;
            // BookIsActive:
            popupBookIsActive.classList.remove(`active`, `deactive`);
            if (bookIsActive === `True` || bookIsActive === `true`) {
                popupBookIsActive.textContent = `Aktiv`;
                popupBookIsActive.classList.remove(`deactive`);
                popupBookIsActive.classList.add(`active`);
            }
            else {
                popupBookIsActive.textContent = `Deaktiv`;
                popupBookIsActive.classList.remove(`active`);
                popupBookIsActive.classList.add(`deactive`);
            }
            // BookCreatedDate:
            popupBookCreatedDate.textContent = bookCreatedDate || `-`;
            // BookUpdatedDate:
            popupBookUpdatedDate.textContent = bookUpdatedDate || `-`;
            // BookImage:
            if (bookImage && bookImage.trim() !== ``) {
                popupBookImage.src = bookImage;
                popupBookImage.classList.add(`visible`);
                popupImageIcon.classList.add(`hidden`);
            }
            else {
                popupBookImage.src = ``;
                popupBookImage.classList.remove(`visible`);
                popupImageIcon.classList.remove(`hidden`);
            }
            // POPUP TEXT DATA END

            // Popupı açılır:
            bookModal.classList.add(`active`);

            // Popup açıq olduqda bodynin scroll edilə bilinməsi bağlanır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        bookModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsi aktivləşir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popup bağlanır:
    document.addEventListener(`keydown`, event => {
        if (event.key === `Escape` && bookModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
