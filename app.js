const birthdayInput = document.getElementById('birthday');
const loadButton = document.getElementById('loadHeadline');
const result = document.getElementById('result');

const formatMonthDay = (dateString) => {
  if (!dateString) return null;
  const [, month, day] = dateString.split('-');
  return `${month}-${day}`;
};

const showMessage = (message) => {
  result.innerHTML = `<p>${message}</p>`;
};

const showHeadline = (monthDay, entry) => {
  result.innerHTML = `
    <h2>${monthDay}</h2>
    <p>${entry.headline}</p>
    <p><a href="${entry.url}" target="_blank" rel="noopener noreferrer">Read related headline links</a></p>
  `;
};

const loadHeadline = async () => {
  const monthDay = formatMonthDay(birthdayInput.value);
  if (!monthDay) {
    showMessage('Please choose your birthday first.');
    return;
  }

  try {
    const response = await fetch('./data/florida_man_headlines.json');
    if (!response.ok) throw new Error('Could not load local headline data.');
    const headlines = await response.json();

    const entry = headlines[monthDay];
    if (!entry) {
      showMessage(`No headline found for ${monthDay}.`);
      return;
    }

    showHeadline(monthDay, entry);
  } catch (error) {
    showMessage(error.message);
  }
};

loadButton.addEventListener('click', loadHeadline);
