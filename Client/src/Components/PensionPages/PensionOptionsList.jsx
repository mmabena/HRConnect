import React, { useEffect, useState } from "react";
import { getPensionOptions } from "../../api/PensionFund";
import "./PensionOptionsList.css";

const PensionOptionsList = () => {
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchPensionOptions();
  }, []);

  const fetchPensionOptions = async () => {
    try {
      const data = await getPensionOptions();
      setOptions(data);
      setLoading(false);
    } catch (err) {
      setError("Failed to load pension options");
      setLoading(false);
    }
  };

  if (loading) return <p className="pension-loading">Loading pension options...</p>;
  if (error) return <p className="pension-error">{error}</p>;

  return (
    <div className="pension-container">
      <h2 className="pension-title">Pension Options</h2>

      {options.length === 0 ? (
        <p className="pension-empty">No pension options available.</p>
      ) : (
        <table className="pension-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Contribution Percentage (%)</th>
            </tr>
          </thead>
          <tbody>
            {options.map((option) => (
              <tr key={option.pensionOptionId}>
                <td>{option.pensionOptionId}</td>
                <td>{option.contributionPercentage}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default PensionOptionsList;