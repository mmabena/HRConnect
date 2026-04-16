import "./Payslip.css"

const SummaryBox=({title,amount,subtext})=>{


  const formatCurrency=(val)=>{
  return new Intl.NumberFormat("en-ZA",{
    style:"currency",
    currency:"ZAR",
  }).format(val||0);
  };

  return(
    <div className="summary-box">
       <p className="summary-title">{title}</p>
       <h2 className="summary-amount">{formatCurrency(amount)}</h2>
       <p className="summary-subtext">{subtext}</p>
    </div>
  );
};
export default SummaryBox;
