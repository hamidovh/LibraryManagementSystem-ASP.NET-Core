/* ~/wwwroot/AdminControllerJSs/UserIndex.js: */

document.addEventListener(`DOMContentLoaded`, () => {
    // USER-MODAL ELEMENTS START
    const userModal = document.getElementById(`userModal`);
    const modalContainer = document.getElementById(`modalContainer`);
    const popupClose = document.getElementById(`popupClose`);
    // USER-MODAL ELEMENTS END

    // POPUP ELEMENTS START
    const popupUserTitle = document.getElementById(`popupUserTitle`);
    const popupUserId = document.getElementById(`popupUserId`);
    const popupUserName = document.getElementById(`popupUserName`);
    const popupUserSurname = document.getElementById(`popupUserSurname`);
    const popupUserDateOfBirth = document.getElementById(`popupUserDateOfBirth`);
    const popupUserSex = document.getElementById(`popupUserSex`);
    const popupUserPersonalNo = document.getElementById(`popupUserPersonalNo`);
    const popupUserEmail = document.getElementById(`popupUserEmail`);
    const popupUserPhoneNo = document.getElementById(`popupUserPhoneNo`);
    const popupUserAddress = document.getElementById(`popupUserAddress`);
    const popupUserUsername = document.getElementById(`popupUserUsername`);
    const popupUserRoleName = document.getElementById(`popupUserRoleName`);
    const popupUserIsActive = document.getElementById(`popupUserIsActive`);
    const popupUserRegistrationDate = document.getElementById(`popupUserRegistrationDate`);
    const popupUserUpdatedDate = document.getElementById(`popupUserUpdatedDate`);
    // POPUP ELEMENTS END

    // DETAILSBUTTON START
    const detailsButtons = document.querySelectorAll(`.btn-details`);

    detailsButtons.forEach((button) => {
        button.addEventListener(`click`, () => {
            // BUTTON DATA START
            const userId = button.dataset.userId;
            const userName = button.dataset.userName;
            const userSurname = button.dataset.userSurname;
            const userDateOfBirth = button.dataset.userDateOfBirth;
            const userSex = button.dataset.userSex;
            const userPersonalNo = button.dataset.userPersonalNo;
            const userEmail = button.dataset.userEmail;
            const userPhoneNo = button.dataset.userPhoneNo;
            const userAddress = button.dataset.userAddress;
            const userUsername = button.dataset.userUsername;
            const userRoleName = button.dataset.userRoleName;
            const userIsActive = button.dataset.userIsActive;
            const userRegistrationDate = button.dataset.userRegistrationDate;
            const userUpdatedDate = button.dataset.userUpdatedDate;
            // BUTTON DATA END

            // POPUP TEXT DATA START
            // UserID:
            popupUserId.textContent = userId || `-`;
            // UserTitle, UserName and UserSurname:
            popupUserTitle.textContent = `${userName || ``} ${userSurname || ``}`.trim() || `İstifadəçi`;
            popupUserName.textContent = userName || `-`;
            popupUserSurname.textContent = userSurname || `-`;
            // UserDateOfBirth:
            popupUserDateOfBirth.textContent = userDateOfBirth || `-`;
            // UserSex:
            popupUserSex.textContent = userSex || `-`;
            // UserPersonalNo:
            popupUserPersonalNo.textContent = userPersonalNo || `-`;
            // UserEmail:
            popupUserEmail.textContent = userEmail || `-`;
            // UserPhoneNo:
            popupUserPhoneNo.textContent = userPhoneNo || `-`;
            // UserAddress:
            popupUserAddress.textContent = userAddress || `-`;
            // UserUsername:
            popupUserUsername.textContent = userUsername || `-`;
            // UserRoleName:
            popupUserRoleName.textContent = userRoleName || `-`;
            // UserIsActive
            popupUserIsActive.classList.remove(`active`, `deactive`);
            if (userIsActive === `True` || userIsActive === `true`) {
                popupUserIsActive.textContent = `Aktiv`;
                popupUserIsActive.classList.add(`active`);
            }
            else {
                popupUserIsActive.textContent = `Deaktiv`;
                popupUserIsActive.classList.add(`deactive`);
            }
            // UserRegistrationDate:
            popupUserRegistrationDate.textContent = userRegistrationDate || `-`;
            // UserUpdatedDate:
            popupUserUpdatedDate.textContent = userUpdatedDate || `-`;
            // POPUP TEXT DATA END

            // Popup açılır:
            userModal.classList.add(`active`);

            // Popup açıq olduqda bodynin scroll edilə bilinməsi bağlanır:
            document.body.style.overflow = `hidden`;
        });
    });
    // DETAILSBUTTON END

    function closeModal() {
        userModal.classList.remove(`active`);

        // Popup bağlandıqda bodynin scroll edilə bilinməsi aktivləşir:
        document.body.style.overflow = ``;
    }

    popupClose.addEventListener(`click`, closeModal);

    modalContainer.addEventListener(`click`, closeModal);

    // Esc düyməsi ilə popup bağlanır:
    document.addEventListener(`keydown`, (event) => {
        if (event.key === `Escape` && userModal.classList.contains(`active`)) {
            closeModal();
        }
    });
});
