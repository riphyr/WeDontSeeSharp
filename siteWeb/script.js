const button = document.getElementById('soundButton');
const audio = document.getElementById('buttonSound');

// Ajouter un gestionnaire d'événements
button.addEventListener('click', () => {
    // Rejouer le son à chaque clic
    audio.currentTime = 0; 
    audio.play();
});
