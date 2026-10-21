/* ~/wwwroot/AdminControllerJSs/FineIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // FINE-MODAL ELEMENTS START
    const fineModal = document.getElementById(`fineModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // FINE-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupFineTitle = document.getElementById(`popupFineTitle`);
    const popupFineId = document.getElementById(`popupFineId`);
    const popupFineUserFullName = document.getElementById(`popupFineUserFullName`);
    const popupFineUserUsername = document.getElementById(`popupFineUserUsername`);
    const popupFineBookName = document.getElementById(`popupFineBookName`);
    const popupFineBookIsbn = document.getElementById(`popupFineBookIsbn`);
    const popupFineReason = document.getElementById(`popupFineReason`);
    const popupFineAmount = document.getElementById(`popupFineAmount`);
    const popupFineStatus = document.getElementById(`popupFineStatus`);
    const popupFineDate = document.getElementById(`popupFineDate`);
    const popupFinePaymentDate = document.getElementById(`popupFinePaymentDate`);
    const popupFineBorrowDate = document.getElementById(`popupFineBorrowDate`);
    const popupFineBorrowDueDate = document.getElementById(`popupFineBorrowDueDate`);
    const popupFineBorrowReturnDate = document.getElementById(`popupFineBorrowReturnDate`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButtons = document.querySelectorAll(`.btn-details`);

    detailsButtons.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const fineId = button.dataset.fineId;
            const userFullName = button.dataset.fineUserFullName;
            const userUsername = button.dataset.fineUserUsername;
            const bookName = button.dataset.fineBookName;
            const bookIsbn = button.dataset.fineBookIsbn;
            const reason = button.dataset.fineReason;
            const amount = button.dataset.fineAmount;
            const fineDate = button.dataset.fineDate;
            const status = button.dataset.fineStatus;
            const paymentDate = button.dataset.finePaymentDate;
            const borrowDate = button.dataset.fineBorrowDate;
            const borrowDueDate = button.dataset.fineBorrowDueDate;
            const borrowReturnDate = button.dataset.fineBorrowReturnDate;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // FineID:
            popupFineId.textContent = fineId || `-`;
            // FineTitle (kitab adı):
            popupFineTitle.textContent = bookName || `Cərimə`;
            // User:
            popupFineUserFullName.textContent = userFullName || `-`;
            popupFineUserUsername.textContent = userUsername || `-`;
            // Book:
            popupFineBookName.textContent = bookName || `-`;
            popupFineBookIsbn.textContent = bookIsbn || `-`;
            // Reason and Amount:
            popupFineReason.textContent = reason || `-`;
            popupFineAmount.textContent = amount || `-`;
            // FineStatus (Ödənilib / Ödənilməyib):
            popupFineStatus.classList.remove(`active`, `deactive`, `overdue`);
            popupFineStatus.textContent = status || `-`;
            if (status === `Ödənilib`) {
                popupFineStatus.classList.add(`active`);
            }
            else {
                popupFineStatus.classList.add(`overdue`);
            }
            // Dates:
            popupFineDate.textContent = fineDate || `-`;
            popupFinePaymentDate.textContent = paymentDate || `-`;
            popupFineBorrowDate.textContent = borrowDate || `-`;
            popupFineBorrowDueDate.textContent = borrowDueDate || `-`;
            popupFineBorrowReturnDate.textContent = borrowReturnDate || `-`;
            // POPUP TEXT DATA END

            // Popup açılır:
            fineModal.classList.add(`active`);

            // Popup açıq olduqda bodynin scroll edilə bilinməsi bağlanır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        fineModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsi aktivləşir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popup bağlanır:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` && fineModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
