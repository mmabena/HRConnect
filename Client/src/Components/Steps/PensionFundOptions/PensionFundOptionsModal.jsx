import React, { useState, useEffect, useRef } from "react";
import "./PensionFundOptionsModal.css";
import { ArrowRight, ArrowLeft } from "lucide-react";

const PensionFundOptionsModal = ({ employee, setEmployee, onNext, onBack }) => {
  const [voluntary, setVoluntary] = useState("");
  const [frequency, setFrequency] = useState("Once-Off");

  const percentageOptions = [2.5, 5, 7.5, 10, 12.5, 15];

  const [selectedPercentage, setSelectedPercentage] = useState(2.5);

  return (
    <div className="emp-leave-container">
      <div className="emp-leave-form-grid">
        {/* HEADING */}
        <div className="emp-pension-fund-personal-details-heading">
          Pension Fund Options
        </div>

        <div className="emp-pension-fund-sub">
          Select funds and contribution rates
        </div>

        {/* SECTION TITLE */}
        <div className="pensio-section-title">PENSION FUND - SELECT ONE</div>

        <div className="emp-leave-type-line" />

        {/* PENSION CARD */}
        <div
          className={`pension-card ${employee?.pensionEnabled ? "active" : ""}`}
        >
          <div
            className="pension-card-header"
            onClick={() =>
              setEmployee((prev) => ({
                ...prev,
                pensionEnabled: !prev.pensionEnabled,
              }))
            }
          >
            <input
              type="checkbox"
              checked={employee?.pensionEnabled || false}
              readOnly
            />

            <div className="pension-header-text">
              <span className="pension-title">Pension Fund</span>

              <span
                className={`pension-subtitle ${
                  employee?.pensionEnabled ? "active" : ""
                }`}
              >
                Select your employee contribution below
              </span>
            </div>
          </div>

          {employee?.pensionEnabled && (
            <>
              <div className="contribution-label">
                EMPLOYEE CONTRIBUTION RATE
              </div>

              <div className="pension-percentages">
                {percentageOptions.map((percentage) => (
                  <button
                    key={percentage}
                    type="button"
                    className={`pension-percent-btn ${
                      selectedPercentage === percentage ? "active" : ""
                    }`}
                    onClick={() => setSelectedPercentage(percentage)}
                  >
                    {percentage}%
                  </button>
                ))}
              </div>
            </>
          )}
        </div>

        {/* VOLUNTARY */}
        <div className="voluntary-section-title">VOLUNTARY CONTRIBUTION</div>
        <div className="emp-leave-type-line" />

        <div className="voluntary-options">
          <button
            type="button"
            className={`voluntary-btn ${
              frequency === "Once-Off" ? "active" : ""
            }`}
            onClick={() => setFrequency("Once-Off")}
          >
            Once-Off
          </button>

          <button
            type="button"
            className={`voluntary-btn ${
              frequency === "Permanent" ? "active" : ""
            }`}
            onClick={() => setFrequency("Permanent")}
          >
            Permanent
          </button>
        </div>

        {/* AMOUNT */}
        <div className="amount-section-title">VOLUNTARY AMOUNT</div>

        <div className="emp-leave-type-line" />
        <input
          type="number"
          placeholder="e.g 500"
          className="voluntary-input"
          value={voluntary}
          onChange={(e) => setVoluntary(e.target.value)}
        />

        {/* FOOTER */}
        <div className="pension-footer">
          <button className="pension-back-btn" onClick={onBack}>
            <ArrowLeft size={18} />
            Back
          </button>

          <button className="pension-next-btn" onClick={onNext}>
            Next
            <ArrowRight size={18} />
          </button>
        </div>
      </div>
    </div>
  );
};

export default PensionFundOptionsModal;
