/* ~/wwwroot/AdminControllerJSs/BorrowIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // BORROW-MODAL ELEMENTS START
    const borrowModal = document.getElementById(`borrowModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // BORROW-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupBorrowTitle = document.getElementById(`popupBorrowTitle`);
    const popupBorrowId = document.getElementById(`popupBorrowId`);
    const popupBorrowUserFullName = document.getElementById(`popupBorrowUserFullName`);
    const popupBorrowUserUsername = document.getElementById(`popupBorrowUserUsername`);
    const popupBorrowBookName = document.getElementById(`popupBorrowBookName`);
    const popupBorrowBookIsbn = document.getElementById(`popupBorrowBookIsbn`);
    const popupBorrowDate = document.getElementById(`popupBorrowDate`);
    const popupBorrowDueDate = document.getElementById(`popupBorrowDueDate`);
    const popupBorrowReturnDate = document.getElementById(`popupBorrowReturnDate`);
    const popupBorrowStatus = document.getElementById(`popupBorrowStatus`);
    const popupBorrowFine = document.getElementById(`popupBorrowFine`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButtons = document.querySelectorAll(`.btn-details`);

    detailsButtons.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const borrowId = button.dataset.borrowId;
            const userFullName = button.dataset.borrowUserFullName;
            const userUsername = button.dataset.borrowUserUsername;
            const bookName = button.dataset.borrowBookName;
            const bookIsbn = button.dataset.borrowBookIsbn;
            const borrowDate = button.dataset.borrowDate;
            const dueDate = button.dataset.borrowDueDate;
            const returnDate = button.dataset.borrowReturnDate;
            const status = button.dataset.borrowStatus;
            const fine = button.dataset.borrowFine;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // BorrowID:
            popupBorrowId.textContent = borrowId || `-`;
            // BorrowTitle (kitab adı):
            popupBorrowTitle.textContent = bookName || `İcarə`;
            // User:
            popupBorrowUserFullName.textContent = userFullName || `-`;
            popupBorrowUserUsername.textContent = userUsername || `-`;
            // Book:
            popupBorrowBookName.textContent = bookName || `-`;
            popupBorrowBookIsbn.textContent = bookIsbn || `-`;
            // Dates:
            popupBorrowDate.textContent = borrowDate || `-`;
            popupBorrowDueDate.textContent = dueDate || `-`;
            popupBorrowReturnDate.textContent = returnDate || `-`;
            // BorrowStatus (Aktiv / Gecikir / Qaytarılıb):
            popupBorrowStatus.classList.remove(`active`, `deactive`, `overdue`);
            popupBorrowStatus.textContent = status || `-`;
            if (status === `Aktiv`) {
                popupBorrowStatus.classList.add(`active`);
            }
            else if (status === `Gecikir`) {
                popupBorrowStatus.classList.add(`overdue`);
            }
            else {
                popupBorrowStatus.classList.add(`deactive`);
            }
            // Fine:
            popupBorrowFine.textContent = fine || `-`;
            // POPUP TEXT DATA END

            // Popup açılır:
            borrowModal.classList.add(`active`);

            // Popup açıq olduqda bodynin scroll edilə bilinməsi bağlanır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        borrowModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsi aktivləşir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popup bağlanır:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` && borrowModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
