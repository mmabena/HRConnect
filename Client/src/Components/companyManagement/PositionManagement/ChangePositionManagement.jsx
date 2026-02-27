import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import api from "../../../api/api";
import { toast } from "react-toastify";
import "../../MenuBar/MenuBar.css";
import "../../../Pages/CompanyManagement/PositionManagement/PositionManagement.css";

const ChangePositionManagement = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { currentPosition, linkedEmployeesCount, attemptedTitle } =
    location.state || {};

  const [moveUsers, setMoveUsers] = useState(false); // default = keep users

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await api.put(`/positions/change/${currentPosition}`, {
        newTitle: attemptedTitle,
        moveUsers,
      });

      toast.success("Position updated successfully.");
      navigate("/positionManagement");
    } catch (error) {
      console.error(error);
      toast.error("Failed to update position.");
    }
  };

return (
  <div className="full-screen-bg">
    <div className="center-frame">
      <div className="left-frame">
        <div className="left-frame-centered">
          <div className="headings-container">
            <div className="apm-logo">
              <span className="apm-logo-bold">singular</span>
              <span className="apm-logo-light">express</span>
            </div>

            <h2 className="apm-title">Change Position</h2>
          </div>

          {/* Position Change Display */}
          <div className="apm-position-change-display">
            <span className="apm-current-position">
              {currentPosition}
            </span>
            <span className="apm-arrow">→</span>
            <span className="apm-new-position">
              {attemptedTitle}
            </span>
          </div>

          <p className="apm-info-text">
            This position currently has <b>{linkedEmployeesCount}</b> users assigned.
          </p>

          <p className="apm-info-text" style={{ marginTop: "20px" }}>
            <b>What would you like to do?</b>
          </p>

          <form onSubmit={handleSubmit} className="apm-form">
            <label className="apm-checkbox-label">
              <input
                type="checkbox"
                checked={!moveUsers}
                onChange={() => setMoveUsers(false)}
              />
              Keep users in current position
            </label>

            <label className="apm-checkbox-label">
              <input
                type="checkbox"
                checked={moveUsers}
                onChange={() => setMoveUsers(true)}
              />
              Move users to a new position
            </label>

            {moveUsers && (
              <div className="apm-view-users">
                <span
                  className="apm-link"
                  onClick={() =>
                    navigate(`/manageUserPosition`, {
                      state: { positionId: currentPosition },
                    })
                  }
                >
                  View Users List &gt;
                </span>
              </div>
            )}

            <button type="submit" className="apm-save-button">
              Save
            </button>

            <div className="apm-footer">
              <p>Privacy Policy | Terms & Conditions</p>
              <p>Copyright © 2025 Singular Systems. All rights reserved.</p>
            </div>
          </form>
        </div>
      </div>

     
        </div>
      
    
  </div>
);
};

export default ChangePositionManagement;