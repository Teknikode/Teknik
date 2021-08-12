// Set your publishable key: remember to change this to your live publishable key in production
// See your keys here: https://dashboard.stripe.com/apikeys
let stripe = Stripe(stripePublishKey);
let elements = stripe.elements();

let card = elements.create('card');
card.mount('#card-element');

card.on('change', function (event) {
    displayError(event);
});
function displayError(event) {
    changeLoadingStatePrices(false);
    let displayError = document.getElementById('card-element-errors');
    if (event.error) {
        displayError.textContent = event.error.message;
    } else {
        displayError.textContent = '';
    }
}