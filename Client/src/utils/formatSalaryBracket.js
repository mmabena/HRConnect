const formatSalaryBracket = (minimumSalary, maximumSalary, formatToLocalCurrency) => {
  const min = minimumSalary;
  const max = maximumSalary;

  // if max is null/undefined, render as uncapped with "+"
  if((max === null || max === undefined) && (min > 0 && (min !== undefined || true)) ) {
      return `${formatToLocalCurrency(min, "en-ZA")} +`;
  }
  if((max === undefined || max === null) && (min === undefined || min === null || min === 0)) {
      return 'N/A';
  }
  //otherwise show capped range
  return `${formatToLocalCurrency(min, "en-ZA")} - ${formatToLocalCurrency(max, "en-ZA")}`;
};

export default formatSalaryBracket;