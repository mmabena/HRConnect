import { useEffect, useState } from "react";
import { getLeaveTypes } from "../../api/leaveTypeApi";
import "./leave-tables.css";
import "../../Components/MenuBar/MenuBar.css";
import NavBar from "../NavBar";
import AddLeaveTypeModal from "./AddLeaveTypeModal";


const LeaveTables = () => {
  const [active, setActive] = useState([]);
  const [inactive, setInactive] = useState([]);
  const [showModal, setShowModal] = useState(false);

  const splitData = (data) => {
    setActive(data.filter(x => x.isActive));
    setInactive(data.filter(x => !x.isActive));
  };
const fetchData = async () => {
  const data = await getLeaveTypes();
  console.log("LEAVE TYPES DATA:", data);
  splitData(data);
};

useEffect(() => {
  fetchData();
}, []);

  // Handles entitlement display logic
  const getEntitlement = (rules) => {
  if (!rules || rules.length === 0) return "-";

  // Get unique days values
  const uniqueDays = [...new Set(rules.map(r => r.daysAllocated))];

  // If all rules have SAME value → show it
  if (uniqueDays.length === 1) {
    return `${uniqueDays[0]} Days`;
  }

  // Different values (Annual Leave) → show dash
  return "-";
};

  return (
     <div className="menu-background custom-scrollbar">

    <div className="wrap-container">
      <div className="heading-container">Company Management</div>
    </div>

    <div className="navbar-with-button">
      <NavBar />
    </div>
  <div className="lt-page-container">


    <div className="lt-top-section">
      <button className="lt-add-btn" onClick={() => setShowModal(true)}>
          + Add Leave Type
      </button>
    </div>

     {/* ================= ACTIVE TABLE ================= */}
      <div className="lt-box">
        <div className="lt-header">
          <span>Leave History</span>
        </div>

        <table className="lt-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Description</th>
              <th>Leave Entitlement</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            {active.map(item => (
              <tr key={item.id}>
                <td className="lt-code">{item.code}</td>
                <td>{item.name}</td>
                <td>{item.description || "-"}</td>
                <td>{getEntitlement(item.rules)}</td>

                <td>
                  <span className="lt-status active">Active</span>
                </td>

                <td className="lt-actions">
                  <span>View</span>
                  <span>Edit</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

   {/* ================= INACTIVE TABLE ================= */}
      <div className="lt-box">
        <div className="lt-header">
          <span>Leave Definitions</span>
        </div>

        <table className="lt-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Description</th>
              <th>Leave Entitlement</th>
              <th>Status</th>
              <th>Comment</th>
            </tr>
          </thead>

          <tbody>
            {inactive.map(item => (
              <tr key={item.id}>
                <td className="lt-code inactive-code">{item.code}</td>
                <td>{item.name}</td>
                <td>{item.description || "-"}</td>
                <td>{getEntitlement(item.rules)}</td>
                <td>
                  <span className="lt-status inactive">Inactive</span>
                </td>
                <td>-</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
  </div> 
  <AddLeaveTypeModal
  isOpen={showModal}
  onClose={() => setShowModal(false)}
  onSuccess={fetchData}
/>
    </div>
  );
};
export default LeaveTables;