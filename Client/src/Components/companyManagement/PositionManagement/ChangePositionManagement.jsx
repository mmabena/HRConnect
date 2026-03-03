import React, { useState, useEffect } from "react";
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

  const [moveUsers, setMoveUsers] = useState(false);

  useEffect(() => {
    if (!currentPosition) {
      navigate("/positionManagement");
    }
  }, [currentPosition, navigate]);

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
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="headings-container">
          <div className="apm-logo">
            <span className="apm-logo-bold">singular</span>
            <span className="apm-logo-light">express</span>
          </div>
          <h2 className="apm-title">Change Position</h2>
        </div>

        <div className="apm-position-change-display">
          <span className="apm-current-position">
            {currentPosition}
          </span>
          <span className="apm-arrow"> → </span>
          <span className="apm-new-position">
            {attemptedTitle}
          </span>
        </div>

        <p className="apm-info-text">
          This position currently has <b>{linkedEmployeesCount}</b> users assigned.
        </p>

        <form onSubmit={handleSubmit} className="apm-form">

          <label className="apm-checkbox-label">
            <input
              type="radio"
              checked={!moveUsers}
              onChange={() => setMoveUsers(false)}
            />
            Keep users in current position
          </label>

          <label className="apm-checkbox-label">
            <input
              type="radio"
              checked={moveUsers}
              onChange={() => setMoveUsers(true)}
            />
            Move users to a new position
          </label>

          {/* ✅ Link appears ONLY when Move Users is selected */}
        {moveUsers && (
  <div className="apm-view-users">
    <span
      className="apm-link"
      onClick={() =>
        navigate(`/manageUserPosition`, {
          state: {
            currentPositionTitle: currentPosition,
            newPositionTitle: attemptedTitle,
          },
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

        </form>
      </div>
    </div>
  );
};

export default ChangePositionManagement;