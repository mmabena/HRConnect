import React, { useState, useEffect } from "react";
import { FaTimes, FaEdit } from "react-icons/fa";
import { resolveRole } from "../../utils/roleUtils";
import { fetchRoles, updateUserRole } from "../../api/UserManagement";
import "./RoleModal.css"
import { fetchAllEmployees } from "../../api/Employee.js";
import { Check, UserRound, UserLock, ArrowRight } from "lucide-react";

const RolesModal = ({ isOpen, onClose, user, onSuccess }) => {
  const [roles, setRoles] = useState([]);
  const [selectedRole, setSelectedRole] = useState(0);
  const [showDropdowns, setShowDropdowns] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [loadedUser, setLoadedUser] = useState({
    firstName: "",
    lastName: ""
  });

  // Fetch roles when modal opens
  useEffect(() => {
    if (!isOpen) return;

    const loadRoles = async () => {
      try {
        const data = await fetchRoles();

        // Normalize roles with consistent keys
        const normalizedRoles = data.map((r) => ({
          id: r.roleId ?? r.id,
          name: r.name,
        }));

        setRoles(normalizedRoles);
      } catch (err) {
        console.error(err);
      }
    };

    loadRoles();
  }, [isOpen]);

  // When user changes, set role and status explicitly as numbers
  useEffect(() => {
    if (user) {
      const normalizedUserRole = resolveRole(user);
      setSelectedRole(normalizedUserRole.roleId != null ? normalizedUserRole.roleId : "",);
      // Backend expects 0 or 1 exactly; coerce here safely
      console.log(`Noramlised User Role ${normalizedUserRole.roleName}`);
      setShowDropdowns(false);
    }
  }, [user]);

  const returnCurrentRole=(roleSelected)=>{
    return roles[roleSelected].name
  }

  const loadUserFromEmployeeData = async () => {
    try {
      const employees = await fetchAllEmployees()
      console.log(employees);

      const employee = employees.find(e => e.email == user.email);
      setLoadedUser({
        firstName: employee?.name || "Unknown User",
        lastName: employee?.surname || "Uknown User"
      })
    }
    catch (error) {
      console.log(`Failed To Load User From Employee Data ${error}`)
      // alert("Failed to load user data. Please try again.");
    }
  }
  useEffect(() => {
    loadUserFromEmployeeData();
  }, [user])

  const handleSave = async () => {
    if (!user?.userId) {
      alert("User not defined");
      return;
    }

    if (selectedRole === "" || Number.isNaN(Number(selectedRole))) {
      alert("Please select a valid role.");
      return;
    }

    setIsSaving(true);

    try {
      const updatedUser = await updateUserRole(user.userId, Number(selectedRole));

      if (onSuccess) onSuccess(updatedUser);
      onClose();
    } catch (error) {
      console.log(error)
      alert(`Error updating user: ${error.message}`);
    } finally {
      setIsSaving(false);
    }
  };

  if (!isOpen || !user) return null;

  return (
    <div className="actions-modal-overlay" onClick={onClose}>
      <div className="actions-modal" onClick={(e) => e.stopPropagation()}>
        <div className="roles-modal-header">
          <div className="name-tag">
            <UserRound className="name-tag-icon" />
            <div>
              <h3>Change User Role</h3>
              <p>
                {loadedUser.firstName} {loadedUser.lastName}
              </p>
            </div>
          </div>
          <button className="close-btn" onClick={onClose}>
            <FaTimes />
          </button>
        </div>
            
        <div className="roles-modal-content">
            <div >
                <p>CURRENT ROLE</p>
                <div className="current-role-banner">
                    <span>
                {selectedRole===0 ? 
                <p className="status normaluser">
                    Normal User
                </p>
                :
                <p className="status superuser">
                    Super User
                </p>
                }
                <p className="status">
                    Change To  <ArrowRight/> 
                </p>
                {selectedRole===1 ?
                 <p className="status normaluser">
                    Normal User
                 </p>
                 :
                  <p className="status superuser">
                    Super User
                  </p>
                 }
                    </span>
                </div>
            </div>
          <div className="roles-buttons">
            <p>SELECT NEW ROLE</p>
            <div className="roles-buttons-wrapper">
              <button
                className={`role-btn superuser ${selectedRole === 1 ? "active" : ""}`}
                onClick={() => {
                  setSelectedRole(1)
                  console.log(`Super User Button Has Role ${selectedRole}`)
                }}>
                <UserLock />
                Super User
              </button>

              <button className={`role-btn normaluser ${selectedRole === 0 ? "active" : ""}`}
                onClick={() => {
                  setSelectedRole(0)
                  console.log(`Normal User Button Has Role ${selectedRole}`)
                }}>
                <UserLock />
                Normal User
              </button>
            </div>
          </div>

          <div className="roles-modal-footer">
            <div className="roles-actions">
              <button className="roles-actions-btn cancel"
                onClick={onClose}>
                Cancel
              </button>
              <button className="roles-actions-btn save"
                onClick={handleSave}
                disabled={isSaving}>
                <Check />
                {isSaving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
};

export default RolesModal;
