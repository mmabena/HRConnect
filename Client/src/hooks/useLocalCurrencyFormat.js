const useLocalCurrencyFormat = () => {
    const toLocalCurrency = (value, locale) =>
        new Intl.NumberFormat(locale, {
            style: "currency",
            currency: "ZAR"
        }).format(value);

    return {
        toLocalCurrency
    };
}

export default useLocalCurrencyFormat;