/* ~/wwwroot/Index/homeindex.js */

const slides = document.querySelectorAll(`.slider-item`);
const prevBtn = document.querySelector(`.slider-control.prev`);
const nextBtn = document.querySelector(`.slider-control.next`);

let currentSlide = 0;

slides[currentSlide].classList.add(`active`);

function showSlide(index) {
  slides[currentSlide].classList.remove(`active`);

  currentSlide = index;

  slides[currentSlide].classList.add(`active`);
}

function nextSlide() {
  let nextIndex = currentSlide + 1;

  if (nextIndex >= slides.length) {
    nextIndex = 0;
  }

  showSlide(nextIndex);
}

function prevSlide() {
  let prevIndex = currentSlide - 1;

  if (prevIndex < 0) {
    prevIndex = slides.length - 1;
  }

  showSlide(prevIndex);
}

nextBtn.addEventListener(`click`, nextSlide);
prevBtn.addEventListener(`click`, prevSlide);

setInterval(nextSlide, 4000);
