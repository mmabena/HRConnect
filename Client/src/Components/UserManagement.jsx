import React, { useState, useEffect } from "react";
// Removed MenuBar import
import Profile from "./MyProfile";
import ActionsModal from "./ActionModal"; // Import the new ActionsModal
import { fetchUsersAndRoles, updateUser } from "../api/UserManagement";
import {
  FaUser,
  FaUsers,
  FaUserLock,
  FaCheckCircle,
  FaTimesCircle,
  FaEdit,
  FaEllipsisV,
  FaTimes,
} from "react-icons/fa";

// Status constants
const USER_STATUS = {
  ACTIVE: 1,
  INACTIVE: 0,
};

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [activeTab, setActiveTab] = useState("myProfile");
  const [currentUserRole, setCurrentUserRole] = useState("User");
  const [selectedUserIndex, setSelectedUserIndex] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [showEditEmployeeModal, setShowEditEmployeeModal] = useState(false);
  const [editRole, setEditRole] = useState("");
  const [editStatus, setEditStatus] = useState(USER_STATUS.ACTIVE);
  const [isLoading, setIsLoading] = useState(true);
  const [loggedInUser, setLoggedInUser] = useState(null);

  useEffect(() => {
    const loadData = async () => {
      try {
        setIsLoading(true);
        const { users, roles } = await fetchUsersAndRoles();

        setRoles(roles || []);
        const mappedUsers = (users || []).map((user) => ({
          ...user,
          name: `${user.firstName || ""} ${user.lastName || ""}`.trim(),
          role: roles.find((r) => r.roleId === user.roleId)?.name || "Unknown Role",
          status: user.status === USER_STATUS.ACTIVE ? "Active" : "Inactive",
          statusValue: user.status,
        }));

        setUsers(mappedUsers);
        if (mappedUsers.length > 0) {
          setLoggedInUser(mappedUsers[0]);
          setCurrentUserRole(mappedUsers[0].role || "User");
        }
      } catch (error) {
        console.error("Failed to load data:", error);
        alert("Failed to load user data. Please try again.");
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, []);

  const hasAdminRights = (role) => ["Admin", "SuperUser"].includes(role || "");

  const handleShowActions = (userIndex) => {
    setSelectedUserIndex(userIndex);
  };

  const handleCloseActions = () => {
    setSelectedUserIndex(null);
  };

  const openEditEmployeeModal = () => {
    if (!hasAdminRights(currentUserRole)) {
      alert("Access denied: Admin rights required");
      return;
    }
    const user = users[selectedUserIndex];
    if (user) {
      setEditRole(user.roleId || ""); // store roleId for select value
      setEditStatus(Number(user.statusValue));
      setShowEditEmployeeModal(true);
      handleCloseActions();
    }
  };

  const saveEmployeeDetails = async () => {
    try {
      const user = users[selectedUserIndex];
      const selectedRole = roles.find((r) => r.roleId === editRole || r.name === editRole);

      if (!user || !selectedRole) throw new Error("Invalid user or role");

      await updateUser(user.userId, {
        roleId: selectedRole.roleId,
        status: editStatus,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
      });

      const updatedUsers = [...users];
      updatedUsers[selectedUserIndex] = {
        ...user,
        role: selectedRole.name,
        status: editStatus === USER_STATUS.ACTIVE ? "Active" : "Inactive",
        statusValue: editStatus,
        roleId: selectedRole.roleId,
      };
      setUsers(updatedUsers);
      setShowEditEmployeeModal(false);
      alert("User updated successfully!");
    } catch (error) {
      console.error("Update failed:", error);
      alert(`Failed to update user: ${error.message}`);
    }
  };

  const handleSaveUser = async (updatedData) => {
    try {
      const user = users[selectedUserIndex];
      if (!user) throw new Error("Invalid user");

      await updateUser(user.userId, {
        ...user,
        ...updatedData,
      });

      const updatedUsers = [...users];
      updatedUsers[selectedUserIndex] = {
        ...user,
        ...updatedData,
        status:
          updatedData.statusValue !== undefined
            ? updatedData.statusValue === USER_STATUS.ACTIVE
              ? "Active"
              : "Inactive"
            : user.status,
      };
      setUsers(updatedUsers);
      return true;
    } catch (error) {
      console.error("Update failed:", error);
      alert(`Failed to update user: ${error.message}`);
      return false;
    }
  };

  const filteredUsers = users.filter((user) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      (user.name || "").toLowerCase().includes(searchLower) ||
      (user.email || "").toLowerCase().includes(searchLower) ||
      (user.role || "").toLowerCase().includes(searchLower)
    );
  });

  if (isLoading)
    return (
      <div className="edit-employee-background">
        {/* Removed MenuBar here */}
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading user data...</p>
        </div>
      </div>
    );

  return (
    <div className="edit-employee-background">
      {/* Removed MenuBar here */}
      <div className="top-bar">
        <FaUsers size={25} />
        <h2>User Management</h2>
      </div>

      <div className="grey-box">
        <div className="top-bar-container">
          <div
            className={`top-bar clickable-tab ${
              activeTab === "myProfile" ? "active-tab" : ""
            }`}
            onClick={() => setActiveTab("myProfile")}
          >
            <FaUser size={20} />
            <h3>My Profile</h3>
          </div>
          <div
            className={`top-bar clickable-tab ${
              activeTab === "userProfile" ? "active-tab" : ""
            }`}
            onClick={() => setActiveTab("userProfile")}
          >
            <FaUser size={20} />
            <h3>User Profiles</h3>
          </div>
        </div>
      </div>

      {activeTab === "myProfile" && <Profile />}

      {activeTab === "userProfile" && (
        <>
          <div className="user-table">
            <div className="search-bar">
              <input
                type="text"
                placeholder=" Search users..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <table className="styled-table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((user, idx) => (
                  <tr key={idx}>
                    <td>
                      <div className="user-info">
                        <div className="avatar-circle">{user.name?.[0] || "?"}</div>
                        <span className="user-name">{user.name || "Unknown User"}</span>
                      </div>
                    </td>
                    <td>{user.email || "No email"}</td>
                    <td>
                      <span className={`role-badge ${(user.role || "").toLowerCase()}`}>
                        <FaUserLock /> {user.role || "Unknown Role"}
                      </span>
                    </td>
                    <td>
                      <span className={`status-badge ${user.status.toLowerCase()}`}>
                        {user.status === "Active" ? <FaCheckCircle /> : <FaTimesCircle />}
                        {user.status}
                      </span>
                    </td>
                    <td className="action-buttons">
                      <button
                        className="actions-trigger-btn"
                        onClick={() => handleShowActions(idx)}
                      >
                        <FaEllipsisV /> Actions
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <ActionsModal
            isOpen={selectedUserIndex !== null}
            onClose={handleCloseActions}
            user={selectedUserIndex !== null ? users[selectedUserIndex] : null}
            onSuccess={() => {
              handleCloseActions();
              // Optionally reload users if needed here
            }}
          />

          {showEditEmployeeModal && selectedUserIndex !== null && (
            <div
              className="actions-modal-overlay"
              onClick={() => setShowEditEmployeeModal(false)}
            >
              <div className="actions-modal" onClick={(e) => e.stopPropagation()}>
                <div className="actions-modal-header">
                  <h3>Edit {users[selectedUserIndex].name}</h3>
                  <button
                    className="close-btn"
                    onClick={() => setShowEditEmployeeModal(false)}
                  >
                    <FaTimes />
                  </button>
                </div>
                <div className="actions-modal-content">
                  <div className="form-group">
                    <label>Role</label>
                    <select
                      value={editRole}
                      onChange={(e) => setEditRole(e.target.value)}
                      disabled={!hasAdminRights(currentUserRole)}
                    >
                      {/* Use roleId as value */}
                      {roles.map((role) => (
                        <option key={role.roleId} value={role.roleId}>
                          {role.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Status</label>
                    <select
                      value={editStatus}
                      onChange={(e) => setEditStatus(Number(e.target.value))}
                      disabled={!hasAdminRights(currentUserRole)}
                    >
                      <option value={USER_STATUS.ACTIVE}>Active</option>
                      <option value={USER_STATUS.INACTIVE}>Inactive</option>
                    </select>
                  </div>
                  <div className="form-actions">
                    <button className="save-btn" onClick={saveEmployeeDetails}>
                      Save Changes
                    </button>
                    <button
                      className="cancel-btn"
                      onClick={() => setShowEditEmployeeModal(false)}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default UserManagement;
