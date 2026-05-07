export const medicalAidOptionDynamicCalculator = (formatToLocalCurrency) => {
    return{
        calculatePrincipalTotal: (risk, msa) => {
            const riskPrincipal = risk;
            const msaPrincipal = msa;
            let totalPrincipal;
            //TODO: fix formatting here especially around where the risk principal will be null, current logic assumes that only the msa might be null
            if(msaPrincipal === 0 || msaPrincipal === null || msaPrincipal === undefined){
                totalPrincipal = Number(riskPrincipal);
            }
            else{
                totalPrincipal = Number(riskPrincipal) + Number(msaPrincipal);
            }

            return `${formatToLocalCurrency(totalPrincipal, "en-ZA")}`;
        },
        calculateAdultTotal: (risk, msa) => {
            const riskAdult = risk;
            const msaAdult = msa;
            let totalAdult;

            if(msaAdult === 0 || msaAdult === null || msaAdult === undefined){
                totalAdult = Number(riskAdult);
            }
            else{
                totalAdult = Number(riskAdult) + Number(msaAdult);
            }

            return `${formatToLocalCurrency(totalAdult, "en-ZA")}`;
        },
        calculateChildTotal: (risk, msa) => {
            const riskChild = risk;
            const msaChild = msa;
            let totalChild;

            if(msaChild === 0 || msaChild === null || msaChild === undefined){
                totalChild = Number(riskChild);
            }
            else{
                totalChild = Number(riskChild) + Number(msaChild);
            }

            return `${formatToLocalCurrency(totalChild, "en-ZA")}`;
        },
        calculateChild2Total: (risk, msa, childFree=false) => {
            const riskChild2 = risk;
            const msaChild2 = msa;
            const free = childFree;
            let totalChild2; // null would be returned for free

            if(free){
                return null;
            }
            if((msaChild2 === 0 || msaChild2 === null || msaChild2 === undefined) && !free){
                totalChild2 = Number(riskChild2);
            }
            else if((msaChild2 !== 0 && msaChild2 !== null && msaChild2 !== undefined) && (riskChild2 !== 0 && riskChild2 !== null && riskChild2 !== undefined) && !free){
                totalChild2 = Number(riskChild2) + Number(msaChild2);
            }

            return `${formatToLocalCurrency(totalChild2, "en-ZA")}`;

        }
    }

    }

export default medicalAidOptionDynamicCalculator;