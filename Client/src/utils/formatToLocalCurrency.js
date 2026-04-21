
    const formatToLocalCurrency = (value, locale) =>
        new Intl.NumberFormat(locale, {
            style: "currency",
            currency: "ZAR"
        }).format(value);


export default formatToLocalCurrency;