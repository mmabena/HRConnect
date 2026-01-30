import React, { useState } from "react";
import {
FaUser,
FaEdit,
FaSave,
FaTimes,
FaPhone,
FaEnvelope,
FaMapMarkerAlt,
FaCalendarAlt,
FaBriefcase,
FaIdCard,
FaUserCircle,
FaCamera,
FaLock,
FaKey,
FaEye,
FaEyeSlash,
} from "react-icons/fa";

const Profile = () => {
const [isEditing, setIsEditing] = useState(false);
const [showPasswordChange, setShowPasswordChange] = useState(false);
const [showPassword, setShowPassword] = useState({
current: false,
new: false,
confirm: false
});

const [profileData, setProfileData] = useState({
firstName: "Miriam",
lastName: "Phily",
email: "mphily@singular.co.za",
phone: "+27 11 123 4567",
address: "123 Main Street, Johannesburg, Gauteng",
dateOfBirth: "1985-06-15",
position: "Software Engineer",
department: "Information Technology",
employeeId: "EMP001",
dateJoined: "2020-03-15",
manager: "John Smith",
bio: "Experienced software engineer with expertise in React and Node.js development.",
emergencyContact: {
name: "John Phily",
relationship: "Spouse",
phone: "+27 11 987 6543"
},
profilePicture: null
});

const [editForm, setEditForm] = useState({...profileData});
const [passwordForm, setPasswordForm] = useState({
currentPassword: "",
newPassword: "",
confirmPassword: ""
});

const handleEditToggle = () => {
if (isEditing) {
setEditForm({...profileData});
}
setIsEditing(!isEditing);
};

const handleSave = () => {
setProfileData({...editForm});
setIsEditing(false);
};

const handleInputChange = (field, value) => {
setEditForm(prev=> ({
...prev,
[field]: value
}));
};

const handleNestedInputChange = (parent, field, value) => {
setEditForm(prev=> ({
...prev,
[parent]: {
...prev[parent],
[field]: value
}
}));
};

const handlePasswordChange = (field, value) => {
setPasswordForm(prev=> ({
...prev,
[field]: value
}));
};

const handlePasswordSubmit = () => {
if (passwordForm.newPassword !== passwordForm.confirmPassword) {
alert("New passwords do not match!");
return;
}
if (passwordForm.newPassword.length < 8) {
alert("Password must be at least 8 characters long!");
return;
}
alert("Password changed successfully!");
setPasswordForm({
currentPassword: "",
newPassword: "",
confirmPassword: ""
});
setShowPasswordChange(false);
};

const togglePasswordVisibility = (field) => {
setShowPassword(prev=> ({
...prev,
[field]: !prev[field]
}));
};

const handleProfilePictureChange = (e) => {
const file = e.target.files[0];
if (file) {
const reader = new FileReader();
reader.onloadend = () => {
setEditForm(prev => ({
...prev,
profilePicture: reader.result
}));
};
reader.readAsDataURL(file);
}
};

return (
<div className="profile-container">
<div className="profile-header">
<div className="profile-picture-section">
<div className="profile-picture">
{profileData.profilePicture ? (
<img
src={profileData.profilePicture}
alt="Profile"
className="profile-image"
/>
) : (
<FaUserCircle size={120} />
)}
{isEditing && (
<div className="picture-upload-container">
<label htmlFor="profile-picture-upload" className="picture-upload-btn">
<FaCamera />
</label>
<input
id="profile-picture-upload"
type="file"
accept="image/*"
onChange={handleProfilePictureChange}
style={{ display: 'none' }}
/>
</div>
)}
</div>
<h2>{profileData.firstName} {profileData.lastName}</h2>
<p className="profile-role">{profileData.position}</p>
</div>


</div>

<div className="profile-content">
<div className="profile-section">
<h3>Personal Information</h3>
<div className="profile-grid">
<div className="profile-field">
<label><FaUser /> First Name</label>
{isEditing ? (
<input
type="text"
value={editForm.firstName}
onChange={(e)=> handleInputChange('firstName', e.target.value)}
/>
) : (
<span>{profileData.firstName}</span>
)}
</div>

<div className="profile-field">
<label><FaUser /> Last Name</label>
{isEditing ? (
<input
type="text"
value={editForm.lastName}
onChange={(e)=> handleInputChange('lastName', e.target.value)}
/>
) : (
<span>{profileData.lastName}</span>
)}
</div>

<div className="profile-field">
<label><FaEnvelope /> Email</label>
<span>{profileData.email}</span>
</div>

<div className="profile-field">
<label><FaPhone /> Phone</label>
{isEditing ? (
<input
type="tel"
value={editForm.phone}
onChange={(e)=> handleInputChange('phone',e.target.value)}
/>
) : (
<span>{profileData.phone}</span>
)}
</div>

<div className="profile-field full-width">
<label><FaMapMarkerAlt /> Address</label>
{isEditing ? (
<input
type="text"
value={editForm.address}
onChange={(e)=> handleInputChange('address', e.target.value)}
/>
) : (
<span>{profileData.address}</span>
)}
</div>

<div className="profile-field">
<label><FaCalendarAlt /> Date of Birth</label>
{isEditing ? (
<input
type="date"
value={editForm.dateOfBirth}
onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
/>
) : (
<span>{new Date(profileData.dateOfBirth).toLocaleDateString()}</span>
)}
</div>
</div>
</div>

<div className="profile-section">
<h3>Work Information</h3>
<div className="profile-grid">
<div className="profile-field">
<label><FaIdCard /> Employee ID</label>
<span>{profileData.employeeId}</span>
</div>

<div className="profile-field">
<label><FaBriefcase /> Position</label>
<span>{profileData.position}</span>
</div>

<div className="profile-field">
<label><FaBriefcase /> Department</label>
<span>{profileData.department}</span>
</div>

<div className="profile-field">
<label><FaCalendarAlt /> Date Joined</label>
<span>{new Date(profileData.dateJoined).toLocaleDateString()}</span>
</div>

<div className="profile-field">
<label><FaUser /> Manager</label>
<span>{profileData.manager}</span>
</div>

<div className="profile-field full-width">
<label>Bio</label>
{isEditing ? (
<textarea
value={editForm.bio}
onChange={(e)=> handleInputChange('bio', e.target.value)}
rows="3"
/>
) : (
<span>{profileData.bio}</span>
)}
</div>
</div>
</div>

<div className="profile-section">
<h3>Emergency Contact</h3>
<div className="profile-grid">
<div className="profile-field">
<label><FaUser /> Name</label>
{isEditing ? (
<input
type="text"
value={editForm.emergencyContact.name}
onChange={(e)=> handleNestedInputChange('emergencyContact', 'name', e.target.value)}
/>
) : (
<span>{profileData.emergencyContact.name}</span>
)}
</div>

<div className="profile-field">
<label>Relationship</label>
{isEditing ? (
<input
type="text"
value={editForm.emergencyContact.relationship}
onChange={(e)=> handleNestedInputChange('emergencyContact', 'relationship', e.target.value)}
/>
) : (
<span>{profileData.emergencyContact.relationship}</span>
)}
</div>

<div className="profile-field">
<label><FaPhone /> Phone</label>
{isEditing ? (
<input
type="tel"
value={editForm.emergencyContact.phone}
onChange={(e)=> handleNestedInputChange('emergencyContact', 'phone', e.target.value)}
/>
) : (
<span>{profileData.emergencyContact.phone}</span>
)}
</div>
</div>
</div>
</div>

{showPasswordChange && (
<div className="password-modal-overlay" onClick={()=> setShowPasswordChange(false)}>
<div className="password-modal" onClick={(e)=> e.stopPropagation()}>
<div className="password-modal-header">
<h3><FaKey /> Change Password</h3>
<button className="close-btn" onClick={() => setShowPasswordChange(false)}>
<FaTimes />
</button>
</div>
<div className="password-modal-content">
<div className="password-form">
<div className="password-field">
<label>Current Password</label>
<div className="password-input-group">
<input
type={showPassword.current ? "text" : "password"}
value={passwordForm.currentPassword}
onChange={(e)=> handlePasswordChange('currentPassword', e.target.value)}
placeholder="Enter current password"
/>
<button
type="button"
className="password-toggle"
onClick={() => togglePasswordVisibility('current')}
>
{showPassword.current ? <FaEyeSlash /> : <FaEye />}
</button>
</div>
</div>

<div className="password-field">
<label>New Password</label>
<div className="password-input-group">
<input
type={showPassword.new ? "text" : "password"}
value={passwordForm.newPassword}
onChange={(e)=> handlePasswordChange('newPassword', e.target.value)}
placeholder="Enter new password"
/>
<button
type="button"
className="password-toggle"
onClick={()=> togglePasswordVisibility('new')}
>
{showPassword.new ? <FaEyeSlash /> : <FaEye />}
</button>
</div>
</div>

<div className="password-field">
<label>Confirm New Password</label>
<div className="password-input-group">
<input
type={showPassword.confirm ? "text" : "password"}
value={passwordForm.confirmPassword}
onChange={(e)=> handlePasswordChange('confirmPassword', e.target.value)}
placeholder="Confirm new password"
/>
<button
type="button"
className="password-toggle"
onClick={()=> togglePasswordVisibility('confirm')}
>
{showPassword.confirm ? <FaEyeSlash /> : <FaEye />}
</button>
</div>
</div>

<div className="password-requirements">
<p>Password must be at least 8 characters long and contain:</p>
<ul>
<li>At least one uppercase letter</li>
<li>At least one lowercase letter</li>
<li>At least one number</li>
<li>At least one special character</li>
</ul>
</div>

<div className="password-actions">
<button className="save-password-btn" onClick={handlePasswordSubmit}>
<FaSave /> Change Password
</button>
<button className="cancel-password-btn" onClick={()=> setShowPasswordChange(false)}>
<FaTimes /> Cancel
</button>
</div>
</div>
</div>
</div>
</div>
)}
</div>
);
};

export default Profile;
