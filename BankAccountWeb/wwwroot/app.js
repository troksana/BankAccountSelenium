const balance = document.querySelector('[data-testid="balance"]');
const amount = document.querySelector('#amount');
const message = document.querySelector('#message');
const money = value => `${Number(value).toFixed(2)} PLN`;
async function refresh() {
  const response = await fetch('/api/balance');
  const data = await response.json();
  balance.textContent = money(data.balance);
}
async function submit(action) {
  message.textContent = '';
  message.className = '';
  const raw = amount.value.trim();
  if (!raw || !Number.isFinite(Number(raw)) || Number(raw) <= 0) {
    message.textContent = 'Amount must be positive.';
    message.className = 'error';
    return;
  }
  try {
    const response = await fetch(`/api/${action}`, {method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({amount:Number(raw)})});
    const data = await response.json();
    if (!response.ok) throw new Error(data.error || 'Request failed.');
    balance.textContent = money(data.balance);
    message.textContent = action === 'deposit' ? 'Deposit successful.' : 'Withdrawal successful.';
    message.className = 'success';
    amount.value = '';
  } catch (error) { message.textContent = error.message; message.className = 'error'; }
}
document.querySelector('#deposit').addEventListener('click', () => submit('deposit'));
document.querySelector('#withdraw').addEventListener('click', () => submit('withdraw'));
refresh();
