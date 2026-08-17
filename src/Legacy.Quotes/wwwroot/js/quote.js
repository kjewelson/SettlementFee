function estimateFee(amount, tier, discountPct, expedited) {
  var rate = amount < 7500 ? 0.0245 : amount < 40000 ? 0.0175 : 0.011;
  if (tier === 'PARTNER') { rate = rate - 0.0025; }
  var fee = Math.round(amount * rate * 100) / 100;
  fee = Math.round(fee * (1 - (discountPct || 0)) * 100) / 100;
  if (fee < window.MIN_FEE) { fee = window.MIN_FEE; }  // set once at page load
  return expedited ? fee + 12.5 : fee;
}
