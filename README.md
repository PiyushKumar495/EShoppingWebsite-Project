# ✨🛒 EShoppingWebsite-Project 🛍️✨

Welcome to **EShoppingWebsite-Project** – a modern, full-stack e-commerce web app built to provide a seamless and delightful online shopping experience. 🚀  
Browse, search, and buy your favorite products with ease!  
Feel free to explore, contribute, or fork this repository. Happy Shopping! 🎁

---

<img src="https://media.giphy.com/media/He9RyyLVJmE9B3F7s7/giphy.gif" alt="Online Shopping Animation" width="500"/>

---

## 🎯 Features

- 🔐 **User Authentication** – Sign up/in securely and manage your account
- 🏷️ **Product Listing** – Browse, filter, and search products by category
- 🥳 **Intuitive Cart** – Add items, view cart, and checkout with ease
- 💳 **Order Management** – Track your orders and view history
- 🛠️ **Admin Panel** – Manage product inventory and orders
- 📱 **Responsive UI** – Looks amazing on all devices
- 💡 **Animations & Emojis** – Interactive and fun user experience thanks to Angular's dynamic features!

---

## 🏗️ Tech Stack

### 🙌 Backend

- Framework: **.NET WebAPI** 🚦
- Database: **MySQL** 🗄️
- Authentication: **JWT**
- API: RESTful endpoints

### 🎨 Frontend

- Framework: **Angular** ⚡
- Styling: **Angular Material, CSS3**
- Animations: **Angular Animations, CSS Transitions**
- HTTP: **Angular HttpClient**

---

## 🎬 Backend Details

- **User APIs:** Register / Login / JWT Auth / Roles
- **Product APIs:** CRUD for products; search & filter
- **Order APIs:** Cart management; order place & track
- **Admin APIs:** Product & order admin routes

```http
POST   /api/auth/register      # New User Register
POST   /api/auth/login         # User Login / Auth
GET    /api/products           # List/Filter Products
POST   /api/orders             # Place Order
GET    /api/admin/orders       # View All Orders (Admin)
```

**Backend Highlights:**
- Built with .NET WebAPI for powerful and secure endpoints.
- MySQL for persistent and scalable data storage.
- JWT-based authentication for secure sessions.
- Separation of concerns with Controllers, Services, and Repositories pattern.

---

## 💻 Frontend Details

- **Angular Single-Page Application** for smooth UX
- **Angular Animations** making navigation and interactions lively 🌟
- **Product Listing**, **Product Details**, **Animated Cart**, **User Auth**, **Admin Panel**
- **Angular Material** for stylish UI components
- **Responsive UI:** Mobile-first, fast and interactive
- **Emojis** and micro-interactions to make shopping more enjoyable!

---

## 🌈 Demo Preview

> Add project screenshots or animated recordings here!

<img src="https://media.giphy.com/media/3o7aD9NGBdqG2yt1vK/giphy.gif" alt="Cart Animation" width="300"/>
<img src="https://media.giphy.com/media/l3Ucl5pIqSaGa82T6/giphy.gif" alt="Product Animation" width="300"/>

---

## 🚀 Quick Start Guide

✨ _Clone, install dependencies, and launch your own shop!_

### 1. Clone this repository

```bash
git clone https://github.com/PiyushKumar495/EShoppingWebsite-Project.git
cd EShoppingWebsite-Project
```

### 2. Backend Setup (.NET)

```bash
cd backend
# Update appsettings.json with your MySQL credentials
dotnet restore
dotnet build
dotnet ef database update    # Run migrations if enabled
dotnet run
```

### 3. Frontend Setup (Angular)

```bash
cd ../frontend
npm install
ng serve
```

### 4. Environment Variables

- Update `appsettings.json` in `backend/` for DB connection, JWT secret.
- Update `environment.ts` in `frontend/` for backend API URL.

---

## 🗂️ Folder Structure

```
EShoppingWebsite-Project/
├── backend/     # .NET WebAPI
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Repositories/
│   ├── appsettings.json
│   └── ...
├── frontend/    # Angular SPA
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   ├── services/
│   │   │   ├── animations/
│   │   │   └── ...
│   │   ├── assets/
│   │   └── ...
└── ...
```

---

## 🎨 Animations & Emojis

- Dynamic page transitions with **Angular Animations**
- Animated cart and checkout flows 🛒✨
- Interactive buttons and form validations
- Fun icons and emoji feedback across the UI!

---

## 📃 License

Licensed under the [MIT License](LICENSE).

---

## 🙋‍♂️ Connect

For feedback, questions, or collaboration:

- GitHub: [PiyushKumar495](https://github.com/PiyushKumar495)
- Issues/PRs welcome! 🚀

---

> _Ready to build or shop? Start exploring and enjoy the modern Angular shopping experience!_ 🤩
