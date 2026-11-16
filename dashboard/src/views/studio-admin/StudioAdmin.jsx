import React, { useEffect, useState } from "react";

const asset = (path) => `/studio-assets/${path}`;
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || "http://localhost:5145";
const adminSessionStorageKey = "zetflix-admin-session";
const adminUnauthorizedEvent = "zetflix-admin-unauthorized";
class AdminApiError extends Error {
  constructor(message, status = 0) {
    super(message);
    this.name = "AdminApiError";
    this.status = status;
  }
}

const getApiLoadErrorMessage = (error, fallback = "showing local seed preview") => {
  if (error?.status === 401) {
    return "Admin session expired. Please sign in again.";
  }

  if (error?.status) {
    return `Backend API returned ${error.status}, ${fallback}. Check the backend console.`;
  }

  return `Backend API is not reachable at ${apiBaseUrl}, ${fallback}.`;
};

const backendAsset = (url) => {
  if (!url) return "";
  if (url.startsWith("http")) return url;
  return `${apiBaseUrl}${url}`;
};
const getStoredAdminSession = () => {
  try {
    return JSON.parse(localStorage.getItem(adminSessionStorageKey) || "null");
  } catch (error) {
    return null;
  }
};
const adminFetch = async (url, options = {}) => {
  const session = getStoredAdminSession();
  const headers = {
    ...(options.headers || {}),
    ...(session?.token ? { Authorization: `Bearer ${session.token}` } : {}),
  };

  const response = await window.fetch(url, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    localStorage.removeItem(adminSessionStorageKey);
    window.dispatchEvent(new Event(adminUnauthorizedEvent));
    throw new AdminApiError("Admin authorization is required.", 401);
  }

  if (!response.ok) {
    throw new AdminApiError(`Admin API returned ${response.status}.`, response.status);
  }

  return response;
};

const sidebarGroups = [
  {
    title: "Navigation",
    items: [
      { label: "Dashboard", page: "dashboard", icon: "fa-laptop" },
      { label: "Movies", page: "movies", icon: "fa-film" },
      { label: "Shows", page: "shows", icon: "fa-tv" },
      { label: "Reviews", page: "reviews", icon: "fa-star" },
    ],
  },
  {
    title: "Catalog",
    items: [
      { label: "Genres", page: "genres", icon: "fa-tags" },
      { label: "Episodes", page: "episodes", icon: "fa-list-ol" },
      { label: "Media Library", page: "media", icon: "fa-photo-film" },
      { label: "Analytics", page: "analytics", icon: "fa-chart-pie" },
    ],
  },
  {
    title: "Users",
    items: [
      { label: "Users", page: "users", icon: "fa-user-circle" },
      { label: "Subscriptions", page: "subscriptions", icon: "fa-credit-card" },
      { label: "Audit Logs", page: "audit", icon: "fa-database" },
      { label: "Settings", page: "settings", icon: "fa-cog" },
    ],
  },
];

const fallbackDashboardStats = {
  movieCount: 3,
  showCount: 2,
  episodeCount: 3,
  userCount: 2,
  reviewCount: 2,
  subscriptionCount: 2,
  revenue: 898,
  averageRating: 5,
  genreBreakdown: [
    { name: "Action", count: 3 },
    { name: "Drama", count: 3 },
    { name: "Comedy", count: 1 },
  ],
  recentReviews: [
    { id: 1, user: "ilya", content: "Inception", rating: 5 },
    { id: 2, user: "ilya", content: "Breaking Bad", rating: 5 },
  ],
};

const fallbackAnalytics = {
  summary: [
    { label: "Catalog items", value: 5, hint: "Movies + shows" },
    { label: "Episodes", value: 3, hint: "Playable series entries" },
    { label: "Users", value: 2, hint: "Registered accounts" },
    { label: "Reviews", value: 2, hint: "5 average rating" },
    { label: "Revenue", value: 898, hint: "Assigned subscriptions" },
  ],
  genres: [
    { name: "Action", movieCount: 2, showCount: 1 },
    { name: "Drama", movieCount: 2, showCount: 1 },
    { name: "Comedy", movieCount: 0, showCount: 1 },
  ],
  ratings: [
    { rating: 5, count: 2 },
  ],
  subscriptions: [
    { name: "Premium", price: 599, userCount: 1, revenue: 599 },
    { name: "Basic", price: 299, userCount: 1, revenue: 299 },
  ],
  contentHealth: [
    { label: "Movies with posters", ready: 3, total: 3 },
    { label: "Shows with posters", ready: 2, total: 2 },
    { label: "Shows with episodes", ready: 2, total: 2 },
    { label: "Reviewed content", ready: 2, total: 5 },
  ],
  roles: [
    { role: "Admin", userCount: 1 },
    { role: "User", userCount: 1 },
  ],
  moderation: {
    totalReviews: 2,
    lowRatingReviews: 0,
    reviewsWithComments: 2,
  },
};

const fallbackSettings = {
  siteName: "Zetflix",
  supportEmail: "gusmanovsemestrovkaorisgusmano@gmail.com",
  heroTitle: "Unlimited movies, shows and more",
  heroSubtitle: "Stream curated content from your Zetflix catalog.",
  allowRegistration: true,
  maintenanceMode: false,
};

const fallbackMovies = [
  { id: 1, title: "Inception", description: "A mind-bending sci-fi thriller.", year: 2010, genreIds: [1, 2], genres: "Action, Drama", status: "Published", rating: 5, posterUrl: "/images/movies/inception.jpg", poster: "img/gallery/gallery-13.jpg" },
  { id: 2, title: "The Dark Knight", description: "Batman faces the Joker in Gotham.", year: 2008, genreIds: [1], genres: "Action", status: "Published", rating: 5, posterUrl: "/images/movies/dark-knight.jpg", poster: "img/gallery/gallery-25.jpg" },
  { id: 3, title: "Interstellar", description: "A journey through space and time.", year: 2014, genreIds: [2], genres: "Drama", status: "Draft", rating: 4, posterUrl: "/images/movies/interstellar.jpg", poster: "img/gallery/gallery-44.jpg" },
];

const fallbackGenres = [
  { id: 1, name: "Action" },
  { id: 2, name: "Drama" },
  { id: 3, name: "Comedy" },
];

const fallbackShows = [
  { id: 1, title: "Breaking Bad", description: "A chemistry teacher turns to crime.", year: 2008, seasons: 1, episodes: 2, genreIds: [2], genres: "Drama", posterUrl: "/images/shows/breaking-bad.jpg", poster: "img/gallery/gallery-32.jpg" },
  { id: 2, title: "Stranger Things", description: "Kids uncover supernatural mysteries.", year: 2016, seasons: 1, episodes: 1, genreIds: [1, 3], genres: "Action, Comedy", posterUrl: "/images/shows/stranger-things.jpg", poster: "img/gallery/gallery-20.jpg" },
];

const fallbackEpisodeLibrary = [
  { id: 1, title: "Pilot", description: "The story begins.", episodeNumber: 1, videoUrl: "/videos/pilot.mp4", seasonId: 1, seasonNumber: 1, showId: 1, showTitle: "Breaking Bad" },
  { id: 2, title: "Cat's in the Bag...", description: "Walter deals with consequences.", episodeNumber: 2, videoUrl: "/videos/cats-in-the-bag.mp4", seasonId: 1, seasonNumber: 1, showId: 1, showTitle: "Breaking Bad" },
  { id: 3, title: "Chapter One", description: "A boy disappears in Hawkins.", episodeNumber: 1, videoUrl: "/videos/chapter-one.mp4", seasonId: 2, seasonNumber: 1, showId: 2, showTitle: "Stranger Things" },
];

const fallbackUsers = [
  { id: 1, name: "admin", email: "admin@zetflix.com", roleId: 1, role: "Admin", subscriptionId: 2, subscription: "Premium", status: "Active" },
  { id: 2, name: "ilya", email: "ilya@zetflix.com", roleId: 2, role: "User", subscriptionId: 1, subscription: "Basic", status: "Active" },
];

const fallbackRoles = [
  { id: 1, name: "Admin" },
  { id: 2, name: "User" },
];

const fallbackSubscriptions = [
  { id: 1, name: "Basic", price: 299, durationInDays: 30, userCount: 1 },
  { id: 2, name: "Premium", price: 599, durationInDays: 30, userCount: 1 },
];

const fallbackReviews = [
  { id: 1, user: "ilya", content: "Inception", type: "Movie", rating: 5, comment: "Amazing movie!" },
  { id: 2, user: "ilya", content: "Breaking Bad", type: "Show", rating: 5, comment: "Very addictive series." },
];

function Sidebar({ activePage, mobileOpen, onSelect, reviewCount }) {
  return (
    <div id="sidebar" className="app-sidebar">
      <div className="app-sidebar-content">
        <div className="menu">
          {sidebarGroups.map((group) => (
            <React.Fragment key={group.title}>
              <div className="menu-header">{group.title}</div>
              {group.items.map((item) => (
                <div key={item.label} className={`menu-item ${activePage === item.page ? "active" : ""} ${item.caret ? "has-sub" : ""}`}>
                  <button type="button" className="menu-link w-100 text-start" onClick={() => onSelect(item.page)}>
                    <span className="menu-icon">
                      <i className={`fa ${item.icon}`}></i>
                      {item.page === "reviews" && reviewCount > 0 && <span className="menu-icon-label">{reviewCount}</span>}
                    </span>
                    <span className="menu-text">{item.label}</span>
                    {item.caret && <span className="menu-caret"><b className="caret"></b></span>}
                  </button>
                </div>
              ))}
            </React.Fragment>
          ))}
          <div className="mt-auto p-4">
            <a href={apiBaseUrl} className="btn btn-secondary w-100 btn-rounded fw-600" target="_blank" rel="noreferrer">
              <i className="fa fa-arrow-up-right-from-square me-2"></i>Public site
            </a>
          </div>
        </div>
      </div>
      {mobileOpen && <div className="app-sidebar-mobile-backdrop"></div>}
    </div>
  );
}

function Header({ onToggle, adminSession, onLogout, onOpenReviews, reviewCount }) {
  return (
    <div id="header" className="app-header">
      <div className="mobile-toggler">
        <button type="button" className="menu-toggler" onClick={onToggle}>
          <span className="bar"></span>
          <span className="bar"></span>
        </button>
      </div>
      <div className="brand">
        <div className="desktop-toggler">
          <button type="button" className="menu-toggler">
            <span className="bar"></span>
            <span className="bar"></span>
          </button>
        </div>
        <a href={apiBaseUrl} className="brand-logo studio-text-logo" target="_blank" rel="noreferrer">Zetflix</a>
      </div>
      <div className="menu">
        <form className="menu-search">
          <div className="menu-search-icon"><i className="fa fa-search"></i></div>
          <div className="menu-search-input">
            <input type="text" className="form-control" placeholder="Search menu..." />
          </div>
        </form>
        <div className="menu-item dropdown">
          <button type="button" className="menu-link border-0 bg-transparent" onClick={onOpenReviews} title="Open review moderation">
            <div className="menu-icon"><i className="fa fa-bell nav-icon"></i></div>
            {reviewCount > 0 && <div className="menu-label">{reviewCount}</div>}
          </button>
        </div>
        <div className="menu-item dropdown">
          <button type="button" className="menu-link border-0 bg-transparent">
            <div className="menu-img online">
              <img src={asset("img/user/user.jpg")} alt="" className="ms-100 mh-100 rounded-circle" />
            </div>
            <div className="menu-text">{adminSession?.email || "admin@zetflix.app"}</div>
          </button>
        </div>
        <div className="menu-item">
          <button type="button" className="menu-link border-0 bg-transparent" onClick={onLogout}>
            <div className="menu-icon"><i className="fa fa-right-from-bracket nav-icon"></i></div>
          </button>
        </div>
      </div>
    </div>
  );
}

function GradientMetric({ card }) {
  return (
    <div className="col-sm-6">
      <div className={`card mb-3 overflow-hidden fs-13px border-0 bg-gradient-custom-${card.color}`} style={{ minHeight: 199 }}>
        <div className="card-img-overlay mb-n4 me-n4 d-flex" style={{ bottom: 0, top: "auto" }}>
          <img src={asset(card.icon)} alt="" className="ms-auto d-block mb-n3" style={{ maxHeight: 105 }} />
        </div>
        <div className="card-body position-relative">
          <h5 className="text-white text-opacity-80 mb-3 fs-16px">{card.title}</h5>
          <h3 className="text-white mt-n1">{card.value}</h3>
          <div className="progress bg-black bg-opacity-50 mb-2" style={{ height: 6 }}>
            <div className="progress-bar progress-bar-striped bg-white" style={{ width: card.width }}></div>
          </div>
          <div className="text-white text-opacity-80 mb-4">
            <i className={`fa fa-caret-${card.up ? "up" : "down"}`}></i> {card.trend} <br />compare to last week
          </div>
        </div>
      </div>
    </div>
  );
}

function WeeklyEarning({ stats, onViewRevenue }) {
  const revenue = Number(stats.revenue || 0);
  const subscriptionRevenue = Math.round(revenue * 0.68);
  const catalogRevenue = revenue - subscriptionRevenue;

  return (
    <div className="card h-100 overflow-hidden">
      <div className="card-img-overlay d-block d-lg-none bg-blue rounded"></div>
      <div
        className="card-img-overlay d-none d-md-block bg-blue rounded mb-n1 mx-n1"
        style={{ backgroundImage: `url(${asset("img/bg/wave-bg.png")})`, backgroundPosition: "right bottom", backgroundRepeat: "no-repeat", backgroundSize: "100%" }}
      ></div>
      <div className="card-img-overlay d-none d-md-block bottom-0 top-auto">
        <div className="row">
          <div className="col-md-8 col-xl-6"></div>
          <div className="col-md-4 col-xl-6 mb-n2">
            <img src={asset("img/page/dashboard.svg")} alt="" className="d-block ms-n3 mb-5" style={{ maxHeight: 310 }} />
          </div>
        </div>
      </div>
      <div className="card-body position-relative text-white text-opacity-70">
        <div className="row">
          <div className="col-md-8">
            <div className="d-flex">
              <div className="me-auto">
                <h5 className="text-white text-opacity-80 mb-3">Subscription Revenue</h5>
                <h3 className="text-white mt-n1 mb-1">{revenue.toLocaleString("en-US")} RUB</h3>
                <p className="mb-1 text-white text-opacity-60 text-truncate">
                  <i className="fa fa-caret-up"></i> <b>{stats.subscriptionCount}</b> active plans in catalog
                </p>
              </div>
            </div>
            <hr className="bg-white bg-opacity-75 mt-3 mb-3" />
            <div className="row">
              <div className="col-6 col-lg-5">
                <div className="mt-1"><i className="fa fa-fw fa-shopping-bag fs-28px text-black text-opacity-50"></i></div>
                <div className="mt-1">
                  <div>Subscriptions</div>
                  <div className="fw-600 text-white">{subscriptionRevenue.toLocaleString("en-US")} RUB</div>
                </div>
              </div>
              <div className="col-6 col-lg-5">
                <div className="mt-1"><i className="fa fa-fw fa-retweet fs-28px text-black text-opacity-50"></i></div>
                <div className="mt-1">
                  <div>Catalog Value</div>
                  <div className="fw-600 text-white">{catalogRevenue.toLocaleString("en-US")} RUB</div>
                </div>
              </div>
            </div>
            <hr className="bg-white bg-opacity-75 mt-3 mb-3" />
            <div className="mt-3 mb-2">
              <button type="button" className="btn btn-yellow btn-rounded btn-sm ps-5 pe-5 pt-2 pb-2 fs-14px fw-600" onClick={onViewRevenue}>
                <i className="fa fa-chart-line me-2 ms-n2"></i> View revenue
              </button>
            </div>
            <p className="fs-12px">Revenue is calculated from users who currently have an assigned subscription.</p>
          </div>
          <div className="col-md-4 d-none d-md-block" style={{ minHeight: 380 }}></div>
        </div>
      </div>
    </div>
  );
}

function SmallStatCard({ stats }) {
  return (
    <div className="card mb-3 flex-1">
      <div className="card-body">
        <div className="d-flex mb-3">
          <div className="flex-grow-1">
            <h5 className="mb-1">Total Users</h5>
            <div>Registered Zetflix accounts</div>
          </div>
          <button type="button" className="btn btn-link text-secondary p-0" aria-label="Refresh users metric"><i className="fa fa-redo"></i></button>
        </div>
        <div className="d-flex">
          <div className="flex-grow-1">
            <h3 className="mb-1">{stats.userCount.toLocaleString("en-US")}</h3>
            <div className="text-success fw-600 fs-13px"><i className="fa fa-caret-up"></i> {stats.subscriptionCount} plans</div>
          </div>
          <div className="w-50px h-50px bg-primary bg-opacity-20 rounded-circle d-flex align-items-center justify-content-center">
            <i className="fa fa-user fa-lg text-primary"></i>
          </div>
        </div>
      </div>
    </div>
  );
}

function SocialCard({ stats }) {
  return (
    <div className="card">
      <div className="card-body">
        <div className="d-flex mb-3">
          <div className="flex-grow-1">
            <h5 className="mb-1">Community</h5>
            <div>Reviews and audience rating</div>
          </div>
          <button type="button" className="btn btn-link text-secondary p-0" aria-label="Refresh community metric"><i className="fa fa-redo"></i></button>
        </div>
        <div className="row">
          {[
            ["fa-comments", stats.reviewCount.toLocaleString("en-US"), "Reviews"],
            ["fa-star", stats.averageRating.toLocaleString("en-US"), "Avg rating"],
          ].map(([icon, count, label]) => (
            <div className="col-6 text-center" key={label}>
              <div className="w-50px h-50px bg-primary bg-opacity-20 rounded-circle d-flex align-items-center justify-content-center mb-2 ms-auto me-auto">
                <i className={`fa ${icon} fa-lg text-primary`}></i>
              </div>
              <div className="fw-600 text-body">{count}</div>
              <div className="fs-13px">{label}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function TrafficCard({ stats }) {
  const totalGenres = stats.genreBreakdown.reduce((sum, genre) => sum + genre.count, 0) || 1;
  const colors = ["primary", "teal", "warning", "danger", "gray-200"];

  return (
    <div className="card h-100">
      <div className="card-body">
        <div className="d-flex mb-3">
          <div className="flex-grow-1">
            <h5 className="mb-1">Genre Mix</h5>
            <div className="fs-13px">Catalog distribution</div>
          </div>
          <button type="button" className="btn btn-link text-secondary p-0" aria-label="Refresh genre mix"><i className="fa fa-redo"></i></button>
        </div>
        <div className="mb-4">
          <h3 className="mb-1">{totalGenres.toLocaleString("en-US")}</h3>
          <div className="text-success fs-13px fw-600"><i className="fa fa-caret-up"></i> genre assignments</div>
        </div>
        <div className="progress mb-4" style={{ height: 10 }}>
          {stats.genreBreakdown.map((genre, index) => (
            <div
              key={genre.name}
              className={`progress-bar bg-${colors[index] || "gray-200"}`}
              style={{ width: `${Math.max((genre.count / totalGenres) * 100, 5)}%` }}
            ></div>
          ))}
        </div>
        <div className="fs-13px">
          {stats.genreBreakdown.map((genre, index) => (
            <div className="d-flex align-items-center mb-2" key={genre.name}>
              <div className="flex-grow-1 d-flex align-items-center">
                <i className={`fa fa-circle fs-9px fa-fw text-${colors[index] || "gray-200"} me-2`}></i> {genre.name}
              </div>
              <div className="fw-600 text-body">{Math.round((genre.count / totalGenres) * 100)}%</div>
            </div>
          ))}
          <div className="fs-12px text-end">
            <span className="fs-10px">powered by Zetflix catalog API</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function SalesAnalytics({ stats }) {
  const chartValues = [
    stats.movieCount,
    stats.showCount,
    stats.episodeCount,
    stats.userCount,
    stats.reviewCount,
    stats.subscriptionCount,
    Math.round(Number(stats.revenue || 0) / 100),
  ];
  const maxValue = Math.max(...chartValues, 1);
  const points = chartValues.map((value, index) => {
    const x = 30 + index * 105;
    const y = 220 - (value / maxValue) * 165;
    return `${x},${y}`;
  }).join(" ");

  return (
    <div className="card h-100">
      <div className="card-body">
        <div className="d-flex mb-3">
          <div className="flex-grow-1">
            <h5 className="mb-1">Catalog Analytics</h5>
            <div className="fs-13px">Current content and business snapshot</div>
          </div>
          <button type="button" className="btn btn-link text-secondary p-0" aria-label="Refresh analytics"><i className="fa fa-redo"></i></button>
        </div>
        <div className="studio-line-chart">
          <div className="studio-grid-line" style={{ bottom: "25%" }}><span>{Math.round(maxValue * 0.25)}</span></div>
          <div className="studio-grid-line" style={{ bottom: "50%" }}><span>{Math.round(maxValue * 0.5)}</span></div>
          <div className="studio-grid-line" style={{ bottom: "75%" }}><span>{Math.round(maxValue * 0.75)}</span></div>
          <svg viewBox="0 0 700 260" preserveAspectRatio="none" aria-hidden="true">
            <polyline points={points} fill="none" stroke="#2f6bff" strokeWidth="7" strokeLinecap="round" />
            <line x1="30" y1="185" x2="660" y2="185" stroke="#8b5cf6" strokeWidth="2" strokeDasharray="2 4" />
          </svg>
          <span className="badge bg-yellow text-black studio-chart-badge earning">Revenue /100</span>
          <span className="badge bg-purple studio-chart-badge support">Catalog</span>
          <div className="studio-chart-dates">
            {["Movies", "Shows", "Episodes", "Users", "Reviews", "Plans", "Rev"].map((label) => <span key={label}>{label}</span>)}
          </div>
        </div>
      </div>
    </div>
  );
}

function Stars({ value }) {
  return (
    <span className="text-warning">
      {Array.from({ length: 5 }, (_, index) => (
        <i key={index} className={`fa${index < value ? "s" : "r"} fa-star me-1`}></i>
      ))}
    </span>
  );
}

function PageTitle({ title, subtitle, action }) {
  return (
    <div className="d-flex align-items-center mb-3">
      <h1 className="page-header mb-0">
        {title} <small>{subtitle}</small>
      </h1>
      {action && <button type="button" className="btn btn-primary ms-auto"><i className="fa fa-plus me-2"></i>{action}</button>}
    </div>
  );
}

function ContentTable({ title, subtitle, children }) {
  return (
    <div className="card">
      <div className="card-body">
        <div className="d-flex align-items-center mb-3">
          <div className="flex-grow-1">
            <h5 className="mb-1">{title}</h5>
            <div className="fs-13px">{subtitle}</div>
          </div>
          <button type="button" className="btn btn-link text-secondary p-0" aria-label="Refresh table"><i className="fa fa-redo"></i></button>
        </div>
        <div className="table-responsive">
          {children}
        </div>
      </div>
    </div>
  );
}

function genreNamesToIds(genres = [], options = fallbackGenres) {
  return genres
    .map((genreName) => options.find((genre) => genre.name === genreName)?.id)
    .filter(Boolean);
}

function mapApiMovie(movie, index, options = fallbackGenres) {
  return {
    id: movie.id,
    title: movie.title,
    description: movie.description || "",
    year: movie.releaseYear,
    genreIds: genreNamesToIds(movie.genres, options),
    genres: movie.genres?.length ? movie.genres.join(", ") : "No genres",
    status: "Published",
    rating: Number(movie.averageRating || 0),
    reviewCount: Number(movie.reviewCount || 0),
    posterUrl: movie.posterUrl || "",
    videoUrl: movie.videoUrl || "",
    poster: fallbackMovies[index % fallbackMovies.length].poster,
  };
}

function MoviesPage() {
  const [movies, setMovies] = useState(fallbackMovies);
  const [genres, setGenres] = useState(fallbackGenres);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [deletingMovieId, setDeletingMovieId] = useState(null);
  const [actionError, setActionError] = useState("");
  const [formError, setFormError] = useState("");
  const [editingMovieId, setEditingMovieId] = useState(null);
  const [formData, setFormData] = useState({
    title: "",
    description: "",
    releaseYear: new Date().getFullYear(),
    posterUrl: "",
    videoUrl: "",
    genreIds: [],
  });

  useEffect(() => {
    let isMounted = true;

    async function loadMovies() {
      try {
        setIsLoading(true);
        setLoadError("");
        const [moviesResponse, genresResponse] = await Promise.all([
          adminFetch(`${apiBaseUrl}/api/admin/movies`),
          adminFetch(`${apiBaseUrl}/api/admin/genres`),
        ]);

        if (!moviesResponse.ok) {
          throw new Error(`Movies API returned ${moviesResponse.status}`);
        }

        if (!genresResponse.ok) {
          throw new Error(`Genres API returned ${genresResponse.status}`);
        }

        const [moviesData, genresData] = await Promise.all([
          moviesResponse.json(),
          genresResponse.json(),
        ]);

        if (isMounted) {
          setGenres(genresData);
          setMovies(moviesData.map((movie, index) => mapApiMovie(movie, index, genresData)));
        }
      } catch (error) {
        if (isMounted) {
          setMovies(fallbackMovies);
          setGenres(fallbackGenres);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadMovies();

    return () => {
      isMounted = false;
    };
  }, []);

  const updateFormField = (field, value) => {
    setFormData((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const toggleGenre = (genreId) => {
    setFormData((current) => ({
      ...current,
      genreIds: current.genreIds.includes(genreId)
        ? current.genreIds.filter((id) => id !== genreId)
        : [...current.genreIds, genreId],
    }));
  };

  const resetForm = () => {
    setFormData({
      title: "",
      description: "",
      releaseYear: new Date().getFullYear(),
      posterUrl: "",
      videoUrl: "",
      genreIds: [],
    });
    setEditingMovieId(null);
    setFormError("");
  };

  const startEditMovie = (movie) => {
    setFormData({
      title: movie.title,
      description: movie.description || "",
      releaseYear: movie.year,
      posterUrl: movie.posterUrl || "",
      videoUrl: movie.videoUrl || "",
      genreIds: movie.genreIds || [],
    });
    setEditingMovieId(movie.id);
    setFormError("");
    setActionError("");
    setIsFormOpen(true);
  };

  const submitMovie = async (event) => {
    event.preventDefault();

    if (!formData.title.trim()) {
      setFormError("Title is required.");
      return;
    }

    if (!formData.description.trim()) {
      setFormError("Description is required.");
      return;
    }

    try {
      setIsSaving(true);
      setFormError("");

      const url = editingMovieId
        ? `${apiBaseUrl}/api/admin/movies/${editingMovieId}`
        : `${apiBaseUrl}/api/admin/movies`;

      const response = await adminFetch(url, {
        method: editingMovieId ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          title: formData.title,
          description: formData.description,
          releaseYear: Number(formData.releaseYear),
          posterUrl: formData.posterUrl || "/images/movies/default.jpg",
          videoUrl: formData.videoUrl || "/assets/images/video/sample-video.mp4",
          genreIds: formData.genreIds,
        }),
      });

      if (!response.ok) {
        throw new Error(`Save movie returned ${response.status}`);
      }

      const savedMovie = await response.json();

      setMovies((current) => {
        const mappedMovie = mapApiMovie(savedMovie, current.length, genres);

        if (editingMovieId) {
          return current.map((movie) => (
            movie.id === editingMovieId ? mappedMovie : movie
          ));
        }

        return [mappedMovie, ...current];
      });

      resetForm();
      setIsFormOpen(false);
    } catch (error) {
      setFormError("Movie was not saved. Check backend connection and try again.");
    } finally {
      setIsSaving(false);
    }
  };

  const deleteMovie = async (movie) => {
    const confirmed = window.confirm(`Delete "${movie.title}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingMovieId(movie.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/movies/${movie.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete movie returned ${response.status}`);
      }

      setMovies((current) => current.filter((item) => item.id !== movie.id));
    } catch (error) {
      setActionError("Movie was not deleted. Check backend connection and try again.");
    } finally {
      setDeletingMovieId(null);
    }
  };

  return (
    <>
      <div className="d-flex align-items-center mb-3">
        <h1 className="page-header mb-0">
          Movies <small>manage Zetflix movie catalog.</small>
        </h1>
        <button
          type="button"
          className="btn btn-primary ms-auto"
          onClick={() => {
            if (isFormOpen) {
              resetForm();
              setIsFormOpen(false);
              return;
            }

            setIsFormOpen(true);
          }}
        >
          <i className="fa fa-plus me-2"></i>
          Add Movie
        </button>
      </div>
  {isFormOpen && (
    <div className="card mb-3">
      <div className="card-body">
        <h5 className="mb-3">{editingMovieId ? "Edit Movie" : "Create Movie"}</h5>

        {formError && (
          <div className="alert alert-danger py-2">
            <i className="fa fa-triangle-exclamation me-2"></i>{formError}
          </div>
        )}

        <form onSubmit={submitMovie}>
          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">Title</label>
              <input
                className="form-control"
                value={formData.title}
                onChange={(event) => updateFormField("title", event.target.value)}
                placeholder="Movie title"
              />
            </div>

            <div className="col-md-3 mb-3">
              <label className="form-label">Release Year</label>
              <input
                type="number"
                className="form-control"
                value={formData.releaseYear}
                onChange={(event) => updateFormField("releaseYear", event.target.value)}
              />
            </div>

            <div className="col-md-3 mb-3">
              <label className="form-label">Poster URL</label>
              <input
                className="form-control"
                value={formData.posterUrl}
                onChange={(event) => updateFormField("posterUrl", event.target.value)}
                placeholder="/images/movies/poster.jpg"
              />
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">Video URL</label>
              <input
                className="form-control"
                value={formData.videoUrl}
                onChange={(event) => updateFormField("videoUrl", event.target.value)}
                placeholder="/assets/images/video/sample-video.mp4"
              />
            </div>
          </div>

          <div className="mb-3">
            <label className="form-label">Description</label>
            <textarea
              className="form-control"
              rows="3"
              value={formData.description}
              onChange={(event) => updateFormField("description", event.target.value)}
              placeholder="Short movie description"
            ></textarea>
          </div>

          <div className="mb-4">
            <label className="form-label d-block">Genres</label>

            {genres.map((genre) => (
              <label className="form-check form-check-inline" key={genre.id}>
                <input
                  type="checkbox"
                  className="form-check-input"
                  checked={formData.genreIds.includes(genre.id)}
                  onChange={() => toggleGenre(genre.id)}
                />
                <span className="form-check-label">{genre.name}</span>
              </label>
            ))}
          </div>

          <div className="d-flex gap-2">
            <button type="submit" className="btn btn-primary" disabled={isSaving}>
              {isSaving ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2"></span>
                  Saving...
                </>
              ) : (
                <>
                  <i className="fa fa-save me-2"></i>
                  {editingMovieId ? "Update Movie" : "Save Movie"}
                </>
              )}
            </button>

            <button
              type="button"
              className="btn btn-default"
              onClick={() => {
                resetForm();
                setIsFormOpen(false);
              }}
              disabled={isSaving}
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  )}

      <ContentTable title="Movie Library" subtitle="Seeded movies from the ASP.NET data model">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}
        {actionError && (
          <div className="alert alert-danger py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
          </div>
        )}
        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>Title</th>
              <th>Year</th>
              <th>Genres</th>
              <th>Status</th>
              <th>Rating</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="6" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading movies...
                </td>
              </tr>
            )}
            {!isLoading && movies.length === 0 && (
              <tr>
                <td colSpan="6" className="text-center py-4">No movies found.</td>
              </tr>
            )}
            {!isLoading && movies.map((movie) => (
              <tr key={movie.id}>
                <td>
                  <div className="d-flex align-items-center">
                    <img src={movie.posterUrl ? backendAsset(movie.posterUrl) : asset(movie.poster)} alt="" className="studio-poster-thumb me-3" />
                    <div>
                      <div className="fw-600 text-body">{movie.title}</div>
                      <div className="fs-13px">Movie #{movie.id}</div>
                    </div>
                  </div>
                </td>
                <td>{movie.year}</td>
                <td>{movie.genres}</td>
                <td><span className={`badge ${movie.status === "Published" ? "bg-success" : "bg-warning"} bg-opacity-20 text-body`}>{movie.status}</span></td>
                <td>
                  <Stars value={Math.round(movie.rating)} />
                  <div className="fs-12px text-body text-opacity-50">
                    {movie.rating ? movie.rating.toFixed(1) : "No rating"} · {movie.reviewCount || 0} reviews
                  </div>
                </td>
                <td className="text-end">
                  <button
                    type="button"
                    className="btn btn-sm btn-default me-1"
                    onClick={() => startEditMovie(movie)}
                  >
                    <i className="fa fa-pen"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingMovieId === movie.id}
                    onClick={() => deleteMovie(movie)}
                  >
                    {deletingMovieId === movie.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function mapApiShow(show, index, options = fallbackGenres) {
  return {
    id: show.id,
    title: show.title,
    description: show.description || "",
    year: show.releaseYear,
    seasons: show.seasonsCount,
    episodes: show.episodesCount,
    genreIds: genreNamesToIds(show.genres, options),
    genres: show.genres?.length ? show.genres.join(", ") : "No genres",
    posterUrl: show.posterUrl || "",
    poster: fallbackShows[index % fallbackShows.length].poster,
  };
}

function ShowsPage() {
  const [shows, setShows] = useState(fallbackShows);
  const [genres, setGenres] = useState(fallbackGenres);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [deletingShowId, setDeletingShowId] = useState(null);
  const [actionError, setActionError] = useState("");
  const [formError, setFormError] = useState("");
  const [editingShowId, setEditingShowId] = useState(null);
  const [selectedShow, setSelectedShow] = useState(null);
  const [seasons, setSeasons] = useState([]);
  const [isSeasonsLoading, setIsSeasonsLoading] = useState(false);
  const [seasonError, setSeasonError] = useState("");
  const [newSeasonNumber, setNewSeasonNumber] = useState(1);
  const [isSeasonSaving, setIsSeasonSaving] = useState(false);
  const [selectedSeason, setSelectedSeason] = useState(null);
  const [episodes, setEpisodes] = useState([]);
  const [isEpisodesLoading, setIsEpisodesLoading] = useState(false);
  const [episodeError, setEpisodeError] = useState("");
  const [isEpisodeSaving, setIsEpisodeSaving] = useState(false);
  const [deletingEpisodeId, setDeletingEpisodeId] = useState(null);
  const [editingEpisodeId, setEditingEpisodeId] = useState(null);
  const [episodeFormData, setEpisodeFormData] = useState({
    title: "",
    description: "",
    episodeNumber: 1,
    videoUrl: "",
  });
  const [formData, setFormData] = useState({
    title: "",
    description: "",
    releaseYear: new Date().getFullYear(),
    posterUrl: "",
    genreIds: [],
  });

  useEffect(() => {
    let isMounted = true;

    async function loadShows() {
      try {
        setIsLoading(true);
        setLoadError("");

        const [showsResponse, genresResponse] = await Promise.all([
          adminFetch(`${apiBaseUrl}/api/admin/shows`),
          adminFetch(`${apiBaseUrl}/api/admin/genres`),
        ]);

        if (!showsResponse.ok) {
          throw new Error(`Shows API returned ${showsResponse.status}`);
        }

        if (!genresResponse.ok) {
          throw new Error(`Genres API returned ${genresResponse.status}`);
        }

        const [showsData, genresData] = await Promise.all([
          showsResponse.json(),
          genresResponse.json(),
        ]);

        if (isMounted) {
          setGenres(genresData);
          setShows(showsData.map((show, index) => mapApiShow(show, index, genresData)));
        }
      } catch (error) {
        if (isMounted) {
          setShows(fallbackShows);
          setGenres(fallbackGenres);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadShows();

    return () => {
      isMounted = false;
    };
  }, []);

  const updateFormField = (field, value) => {
    setFormData((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const toggleGenre = (genreId) => {
    setFormData((current) => ({
      ...current,
      genreIds: current.genreIds.includes(genreId)
        ? current.genreIds.filter((id) => id !== genreId)
        : [...current.genreIds, genreId],
    }));
  };

  const resetForm = () => {
    setFormData({
      title: "",
      description: "",
      releaseYear: new Date().getFullYear(),
      posterUrl: "",
      genreIds: [],
    });
    setEditingShowId(null);
    setFormError("");
  };

  const startEditShow = (show) => {
    setFormData({
      title: show.title,
      description: show.description || "",
      releaseYear: show.year,
      posterUrl: show.posterUrl || "",
      genreIds: show.genreIds || [],
    });
    setEditingShowId(show.id);
    setFormError("");
    setActionError("");
    setIsFormOpen(true);
  };

  const submitShow = async (event) => {
    event.preventDefault();

    if (!formData.title.trim()) {
      setFormError("Title is required.");
      return;
    }

    if (!formData.description.trim()) {
      setFormError("Description is required.");
      return;
    }

    try {
      setIsSaving(true);
      setFormError("");

      const url = editingShowId
        ? `${apiBaseUrl}/api/admin/shows/${editingShowId}`
        : `${apiBaseUrl}/api/admin/shows`;

      const response = await adminFetch(url, {
        method: editingShowId ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          title: formData.title,
          description: formData.description,
          releaseYear: Number(formData.releaseYear),
          posterUrl: formData.posterUrl || "/images/shows/default.jpg",
          genreIds: formData.genreIds,
        }),
      });

      if (!response.ok) {
        throw new Error(`Save show returned ${response.status}`);
      }

      const savedShow = await response.json();

      setShows((current) => {
        const mappedShow = mapApiShow(savedShow, current.length, genres);

        if (editingShowId) {
          return current.map((show) => (
            show.id === editingShowId ? mappedShow : show
          ));
        }

        return [mappedShow, ...current];
      });

      resetForm();
      setIsFormOpen(false);
    } catch (error) {
      setFormError("Show was not saved. Check backend connection and try again.");
    } finally {
      setIsSaving(false);
    }
  };

  const deleteShow = async (show) => {
    const confirmed = window.confirm(`Delete "${show.title}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingShowId(show.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/shows/${show.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete show returned ${response.status}`);
      }

      setShows((current) => current.filter((item) => item.id !== show.id));
      if (selectedShow?.id === show.id) {
        setSelectedShow(null);
        setSeasons([]);
        setSelectedSeason(null);
        setEpisodes([]);
      }
    } catch (error) {
      setActionError("Show was not deleted. Check backend connection and try again.");
    } finally {
      setDeletingShowId(null);
    }
  };

  const loadSeasons = async (show) => {
    try {
      setSelectedShow(show);
      setIsSeasonsLoading(true);
      setSeasonError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/shows/${show.id}/seasons`);

      if (!response.ok) {
        throw new Error(`Seasons API returned ${response.status}`);
      }

      const data = await response.json();
      setSeasons(data);

      const nextNumber = data.length
        ? Math.max(...data.map((season) => season.number)) + 1
        : 1;
      setNewSeasonNumber(nextNumber);
    } catch (error) {
      setSeasons([]);
      setSelectedSeason(null);
      setEpisodes([]);
      setSeasonError("Seasons were not loaded. Check backend connection and try again.");
    } finally {
      setIsSeasonsLoading(false);
    }
  };

  const createSeason = async (event) => {
    event.preventDefault();

    if (!selectedShow) {
      return;
    }

    const parsedNumber = Number(newSeasonNumber);

    if (!Number.isInteger(parsedNumber) || parsedNumber <= 0) {
      setSeasonError("Season number must be greater than zero.");
      return;
    }

    try {
      setIsSeasonSaving(true);
      setSeasonError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/shows/${selectedShow.id}/seasons`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          number: parsedNumber,
        }),
      });

      if (!response.ok) {
        throw new Error(`Create season returned ${response.status}`);
      }

      const createdSeason = await response.json();

      setSeasons((current) => {
        const nextSeasons = [...current, createdSeason].sort((left, right) => left.number - right.number);
        setNewSeasonNumber(Math.max(...nextSeasons.map((season) => season.number)) + 1);
        return nextSeasons;
      });
      setShows((current) => current.map((show) => (
        show.id === selectedShow.id
          ? { ...show, seasons: show.seasons + 1 }
          : show
      )));
      setSelectedShow((current) => (
        current ? { ...current, seasons: current.seasons + 1 } : current
      ));
    } catch (error) {
      setSeasonError("Season was not created. It may already exist or backend is unavailable.");
    } finally {
      setIsSeasonSaving(false);
    }
  };

  const resetEpisodeForm = () => {
    setEpisodeFormData({
      title: "",
      description: "",
      episodeNumber: episodes.length ? Math.max(...episodes.map((episode) => episode.episodeNumber)) + 1 : 1,
      videoUrl: "",
    });
    setEditingEpisodeId(null);
    setEpisodeError("");
  };

  const updateEpisodeField = (field, value) => {
    setEpisodeFormData((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const loadEpisodes = async (season) => {
    try {
      setSelectedSeason(season);
      setIsEpisodesLoading(true);
      setEpisodeError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/seasons/${season.id}/episodes`);

      if (!response.ok) {
        throw new Error(`Episodes API returned ${response.status}`);
      }

      const data = await response.json();
      setEpisodes(data);
      setEpisodeFormData({
        title: "",
        description: "",
        episodeNumber: data.length ? Math.max(...data.map((episode) => episode.episodeNumber)) + 1 : 1,
        videoUrl: "",
      });
      setEditingEpisodeId(null);
    } catch (error) {
      setEpisodes([]);
      setEpisodeError("Episodes were not loaded. Check backend connection and try again.");
    } finally {
      setIsEpisodesLoading(false);
    }
  };

  const startEditEpisode = (episode) => {
    setEpisodeFormData({
      title: episode.title,
      description: episode.description || "",
      episodeNumber: episode.episodeNumber,
      videoUrl: episode.videoUrl || "",
    });
    setEditingEpisodeId(episode.id);
    setEpisodeError("");
  };

  const submitEpisode = async (event) => {
    event.preventDefault();

    if (!selectedSeason) {
      return;
    }

    if (!episodeFormData.title.trim()) {
      setEpisodeError("Title is required.");
      return;
    }

    if (!episodeFormData.description.trim()) {
      setEpisodeError("Description is required.");
      return;
    }

    if (!episodeFormData.videoUrl.trim()) {
      setEpisodeError("Video URL is required.");
      return;
    }

    try {
      setIsEpisodeSaving(true);
      setEpisodeError("");

      const url = editingEpisodeId
        ? `${apiBaseUrl}/api/admin/seasons/${selectedSeason.id}/episodes/${editingEpisodeId}`
        : `${apiBaseUrl}/api/admin/seasons/${selectedSeason.id}/episodes`;

      const response = await adminFetch(url, {
        method: editingEpisodeId ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          title: episodeFormData.title,
          description: episodeFormData.description,
          episodeNumber: Number(episodeFormData.episodeNumber),
          videoUrl: episodeFormData.videoUrl,
        }),
      });

      if (!response.ok) {
        throw new Error(`Save episode returned ${response.status}`);
      }

      const savedEpisode = await response.json();

      setEpisodes((current) => {
        if (editingEpisodeId) {
          return current
            .map((episode) => episode.id === editingEpisodeId ? savedEpisode : episode)
            .sort((left, right) => left.episodeNumber - right.episodeNumber);
        }

        return [...current, savedEpisode].sort((left, right) => left.episodeNumber - right.episodeNumber);
      });

      if (!editingEpisodeId) {
        setSeasons((current) => current.map((season) => (
          season.id === selectedSeason.id
            ? { ...season, episodesCount: season.episodesCount + 1 }
            : season
        )));
        setSelectedSeason((current) => (
          current ? { ...current, episodesCount: current.episodesCount + 1 } : current
        ));
        setShows((current) => current.map((show) => (
          selectedShow && show.id === selectedShow.id
            ? { ...show, episodes: show.episodes + 1 }
            : show
        )));
      }

      resetEpisodeForm();
    } catch (error) {
      setEpisodeError("Episode was not saved. It may duplicate a number or backend is unavailable.");
    } finally {
      setIsEpisodeSaving(false);
    }
  };

  const deleteEpisode = async (episode) => {
    if (!selectedSeason) {
      return;
    }

    const confirmed = window.confirm(`Delete "${episode.title}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingEpisodeId(episode.id);
      setEpisodeError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/seasons/${selectedSeason.id}/episodes/${episode.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete episode returned ${response.status}`);
      }

      setEpisodes((current) => current.filter((item) => item.id !== episode.id));
      setSeasons((current) => current.map((season) => (
        season.id === selectedSeason.id
          ? { ...season, episodesCount: Math.max(season.episodesCount - 1, 0) }
          : season
      )));
      setSelectedSeason((current) => (
        current ? { ...current, episodesCount: Math.max(current.episodesCount - 1, 0) } : current
      ));
      setShows((current) => current.map((show) => (
        selectedShow && show.id === selectedShow.id
          ? { ...show, episodes: Math.max(show.episodes - 1, 0) }
          : show
      )));
      if (editingEpisodeId === episode.id) {
        resetEpisodeForm();
      }
    } catch (error) {
      setEpisodeError("Episode was not deleted. Check backend connection and try again.");
    } finally {
      setDeletingEpisodeId(null);
    }
  };

  return (
    <>
      <div className="d-flex align-items-center mb-3">
        <h1 className="page-header mb-0">
          Shows <small>manage series, seasons and episodes.</small>
        </h1>
        <button
          type="button"
          className="btn btn-primary ms-auto"
          onClick={() => {
            if (isFormOpen) {
              resetForm();
              setIsFormOpen(false);
              return;
            }

            setIsFormOpen(true);
          }}
        >
          <i className="fa fa-plus me-2"></i>
          Add Show
        </button>
      </div>

      {isFormOpen && (
        <div className="card mb-3">
          <div className="card-body">
            <h5 className="mb-3">{editingShowId ? "Edit Show" : "Create Show"}</h5>

            {formError && (
              <div className="alert alert-danger py-2">
                <i className="fa fa-triangle-exclamation me-2"></i>{formError}
              </div>
            )}

            <form onSubmit={submitShow}>
              <div className="row">
                <div className="col-md-6 mb-3">
                  <label className="form-label">Title</label>
                  <input
                    className="form-control"
                    value={formData.title}
                    onChange={(event) => updateFormField("title", event.target.value)}
                    placeholder="Show title"
                  />
                </div>

                <div className="col-md-3 mb-3">
                  <label className="form-label">Release Year</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.releaseYear}
                    onChange={(event) => updateFormField("releaseYear", event.target.value)}
                  />
                </div>

                <div className="col-md-3 mb-3">
                  <label className="form-label">Poster URL</label>
                  <input
                    className="form-control"
                    value={formData.posterUrl}
                    onChange={(event) => updateFormField("posterUrl", event.target.value)}
                    placeholder="/images/shows/poster.jpg"
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Description</label>
                <textarea
                  className="form-control"
                  rows="3"
                  value={formData.description}
                  onChange={(event) => updateFormField("description", event.target.value)}
                  placeholder="Short show description"
                ></textarea>
              </div>

              <div className="mb-4">
                <label className="form-label d-block">Genres</label>
                {genres.map((genre) => (
                  <label className="form-check form-check-inline" key={genre.id}>
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={formData.genreIds.includes(genre.id)}
                      onChange={() => toggleGenre(genre.id)}
                    />
                    <span className="form-check-label">{genre.name}</span>
                  </label>
                ))}
              </div>

              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary" disabled={isSaving}>
                  {isSaving ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      <i className="fa fa-save me-2"></i>
                      {editingShowId ? "Update Show" : "Save Show"}
                    </>
                  )}
                </button>

                <button
                  type="button"
                  className="btn btn-default"
                  onClick={() => {
                    resetForm();
                    setIsFormOpen(false);
                  }}
                  disabled={isSaving}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <ContentTable title="Series Library" subtitle="Shows connected to seasons and episodes">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}
        {actionError && (
          <div className="alert alert-danger py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
          </div>
        )}

        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>Title</th>
              <th>Year</th>
              <th>Genres</th>
              <th>Seasons</th>
              <th>Episodes</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="6" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading shows...
                </td>
              </tr>
            )}

            {!isLoading && shows.length === 0 && (
              <tr>
                <td colSpan="6" className="text-center py-4">No shows found.</td>
              </tr>
            )}

            {!isLoading && shows.map((show) => (
              <tr key={show.id}>
                <td>
                  <div className="d-flex align-items-center">
                    <img src={show.posterUrl ? backendAsset(show.posterUrl) : asset(show.poster)} alt="" className="studio-poster-thumb me-3" />
                    <div>
                      <div className="fw-600 text-body">{show.title}</div>
                      <div className="fs-13px">Show #{show.id}</div>
                    </div>
                  </div>
                </td>
                <td>{show.year}</td>
                <td>{show.genres}</td>
                <td>{show.seasons}</td>
                <td>{show.episodes}</td>
                <td className="text-end">
                  <button
                    type="button"
                    className="btn btn-sm btn-default me-1"
                    onClick={() => loadSeasons(show)}
                  >
                    <i className="fa fa-layer-group"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-default me-1"
                    onClick={() => startEditShow(show)}
                  >
                    <i className="fa fa-pen"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingShowId === show.id}
                    onClick={() => deleteShow(show)}
                  >
                    {deletingShowId === show.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
      {selectedShow && (
        <div className="card mt-3">
          <div className="card-body">
            <div className="d-flex align-items-center mb-3">
              <div className="flex-grow-1">
                <h5 className="mb-1">{selectedShow.title} Seasons</h5>
                <div className="fs-13px">Manage seasons before adding episodes.</div>
              </div>
              <button
                type="button"
                className="btn btn-sm btn-default"
                onClick={() => {
                  setSelectedShow(null);
                  setSeasons([]);
                  setSelectedSeason(null);
                  setEpisodes([]);
                  setSeasonError("");
                  setEpisodeError("");
                }}
              >
                Close
              </button>
            </div>

            {seasonError && (
              <div className="alert alert-danger py-2 mb-3">
                <i className="fa fa-triangle-exclamation me-2"></i>{seasonError}
              </div>
            )}

            <form className="row align-items-end mb-3" onSubmit={createSeason}>
              <div className="col-md-3 mb-2 mb-md-0">
                <label className="form-label">Season Number</label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  value={newSeasonNumber}
                  onChange={(event) => setNewSeasonNumber(event.target.value)}
                />
              </div>
              <div className="col-md-3">
                <button type="submit" className="btn btn-primary" disabled={isSeasonSaving}>
                  {isSeasonSaving ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      <i className="fa fa-plus me-2"></i>
                      Add Season
                    </>
                  )}
                </button>
              </div>
            </form>

            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>Season</th>
                    <th>Episodes</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {isSeasonsLoading && (
                    <tr>
                      <td colSpan="3" className="text-center py-4">
                        <span className="spinner-border spinner-border-sm me-2"></span>Loading seasons...
                      </td>
                    </tr>
                  )}
                  {!isSeasonsLoading && seasons.length === 0 && (
                    <tr>
                      <td colSpan="3" className="text-center py-4">No seasons found.</td>
                    </tr>
                  )}
                  {!isSeasonsLoading && seasons.map((season) => (
                    <tr key={season.id}>
                      <td className="fw-600 text-body">Season {season.number}</td>
                      <td>{season.episodesCount}</td>
                      <td className="text-end">
                        <button
                          type="button"
                          className="btn btn-sm btn-default"
                          onClick={() => loadEpisodes(season)}
                        >
                          Episodes
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
      {selectedSeason && (
        <div className="card mt-3">
          <div className="card-body">
            <div className="d-flex align-items-center mb-3">
              <div className="flex-grow-1">
                <h5 className="mb-1">
                  {selectedShow?.title} / Season {selectedSeason.number} Episodes
                </h5>
                <div className="fs-13px">Manage episode metadata and video URLs.</div>
              </div>
              <button
                type="button"
                className="btn btn-sm btn-default"
                onClick={() => {
                  setSelectedSeason(null);
                  setEpisodes([]);
                  resetEpisodeForm();
                }}
              >
                Close
              </button>
            </div>

            {episodeError && (
              <div className="alert alert-danger py-2 mb-3">
                <i className="fa fa-triangle-exclamation me-2"></i>{episodeError}
              </div>
            )}

            <form className="mb-3" onSubmit={submitEpisode}>
              <div className="row">
                <div className="col-md-5 mb-3">
                  <label className="form-label">Title</label>
                  <input
                    className="form-control"
                    value={episodeFormData.title}
                    onChange={(event) => updateEpisodeField("title", event.target.value)}
                    placeholder="Episode title"
                  />
                </div>
                <div className="col-md-2 mb-3">
                  <label className="form-label">Episode #</label>
                  <input
                    type="number"
                    min="1"
                    className="form-control"
                    value={episodeFormData.episodeNumber}
                    onChange={(event) => updateEpisodeField("episodeNumber", event.target.value)}
                  />
                </div>
                <div className="col-md-5 mb-3">
                  <label className="form-label">Video URL</label>
                  <input
                    className="form-control"
                    value={episodeFormData.videoUrl}
                    onChange={(event) => updateEpisodeField("videoUrl", event.target.value)}
                    placeholder="/videos/episode.mp4"
                  />
                </div>
              </div>
              <div className="mb-3">
                <label className="form-label">Description</label>
                <textarea
                  className="form-control"
                  rows="3"
                  value={episodeFormData.description}
                  onChange={(event) => updateEpisodeField("description", event.target.value)}
                  placeholder="Episode description"
                ></textarea>
              </div>
              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary" disabled={isEpisodeSaving}>
                  {isEpisodeSaving ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      <i className="fa fa-save me-2"></i>
                      {editingEpisodeId ? "Update Episode" : "Save Episode"}
                    </>
                  )}
                </button>
                <button
                  type="button"
                  className="btn btn-default"
                  onClick={resetEpisodeForm}
                  disabled={isEpisodeSaving}
                >
                  Cancel
                </button>
              </div>
            </form>

            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Title</th>
                    <th>Video URL</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {isEpisodesLoading && (
                    <tr>
                      <td colSpan="4" className="text-center py-4">
                        <span className="spinner-border spinner-border-sm me-2"></span>Loading episodes...
                      </td>
                    </tr>
                  )}
                  {!isEpisodesLoading && episodes.length === 0 && (
                    <tr>
                      <td colSpan="4" className="text-center py-4">No episodes found.</td>
                    </tr>
                  )}
                  {!isEpisodesLoading && episodes.map((episode) => (
                    <tr key={episode.id}>
                      <td>{episode.episodeNumber}</td>
                      <td>
                        <div className="fw-600 text-body">{episode.title}</div>
                        <div className="fs-13px">{episode.description}</div>
                      </td>
                      <td>{episode.videoUrl}</td>
                      <td className="text-end">
                        <button
                          type="button"
                          className="btn btn-sm btn-default me-1"
                          onClick={() => startEditEpisode(episode)}
                        >
                          <i className="fa fa-pen"></i>
                        </button>
                        <button
                          type="button"
                          className="btn btn-sm btn-danger"
                          disabled={deletingEpisodeId === episode.id}
                          onClick={() => deleteEpisode(episode)}
                        >
                          {deletingEpisodeId === episode.id ? (
                            <span className="spinner-border spinner-border-sm"></span>
                          ) : (
                            <i className="fa fa-trash"></i>
                          )}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function mapApiUser(user) {
  return {
    id: user.id,
    name: user.userName,
    email: user.email,
    roleId: user.roleId,
    role: user.role,
    subscriptionId: user.subscriptionId,
    subscription: user.subscription,
    status: user.status,
  };
}

function UsersPage() {
  const [users, setUsers] = useState(fallbackUsers);
  const [roles, setRoles] = useState(fallbackRoles);
  const [subscriptions, setSubscriptions] = useState(fallbackSubscriptions);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [deletingUserId, setDeletingUserId] = useState(null);
  const [editingUserId, setEditingUserId] = useState(null);
  const [formData, setFormData] = useState({
    userName: "",
    email: "",
    roleId: fallbackRoles[0].id,
    subscriptionId: "",
  });

  useEffect(() => {
    let isMounted = true;

    async function loadUsers() {
      try {
        setIsLoading(true);
        setLoadError("");

        const [usersResponse, optionsResponse] = await Promise.all([
          adminFetch(`${apiBaseUrl}/api/admin/users`),
          adminFetch(`${apiBaseUrl}/api/admin/users/options`),
        ]);

        if (!usersResponse.ok) {
          throw new Error(`Users API returned ${usersResponse.status}`);
        }

        if (!optionsResponse.ok) {
          throw new Error(`User options API returned ${optionsResponse.status}`);
        }

        const [usersData, optionsData] = await Promise.all([
          usersResponse.json(),
          optionsResponse.json(),
        ]);

        if (isMounted) {
          setUsers(usersData.map(mapApiUser));
          setRoles(optionsData.roles);
          setSubscriptions(optionsData.subscriptions);
        }
      } catch (error) {
        if (isMounted) {
          setUsers(fallbackUsers);
          setRoles(fallbackRoles);
          setSubscriptions(fallbackSubscriptions);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadUsers();

    return () => {
      isMounted = false;
    };
  }, []);

  const selectedUser = users.find((user) => user.id === editingUserId);

  function startEditUser(user) {
    setActionError("");
    setEditingUserId(user.id);
    setFormData({
      userName: user.name,
      email: user.email,
      roleId: user.roleId || roles[0]?.id || "",
      subscriptionId: user.subscriptionId || "",
    });
  }

  function cancelEditUser() {
    setActionError("");
    setEditingUserId(null);
    setFormData({
      userName: "",
      email: "",
      roleId: roles[0]?.id || "",
      subscriptionId: "",
    });
  }

  function updateFormField(event) {
    const { name, value } = event.target;
    setFormData((currentFormData) => ({
      ...currentFormData,
      [name]: value,
    }));
  }

  async function saveUser(event) {
    event.preventDefault();

    if (!editingUserId) {
      return;
    }

    try {
      setIsSaving(true);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/users/${editingUserId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userName: formData.userName,
          email: formData.email,
          roleId: Number(formData.roleId),
          subscriptionId: formData.subscriptionId ? Number(formData.subscriptionId) : null,
        }),
      });

      if (!response.ok) {
        throw new Error(`Update user API returned ${response.status}`);
      }

      const updatedUser = mapApiUser(await response.json());

      setUsers((currentUsers) => currentUsers.map((user) => (
        user.id === updatedUser.id ? updatedUser : user
      )));
      cancelEditUser();
    } catch (error) {
      setActionError("Could not update user. Check the backend API and try again.");
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteUser(user) {
    if (!window.confirm(`Delete user ${user.name}? Their reviews will also be removed.`)) {
      return;
    }

    try {
      setDeletingUserId(user.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/users/${user.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete user API returned ${response.status}`);
      }

      setUsers((currentUsers) => currentUsers.filter((currentUser) => currentUser.id !== user.id));

      if (editingUserId === user.id) {
        cancelEditUser();
      }
    } catch (error) {
      setActionError(error?.status === 400
        ? "Could not delete this user. The last admin account is protected."
        : "Could not delete user. Check the backend API and try again.");
    } finally {
      setDeletingUserId(null);
    }
  }

  return (
    <>
      <PageTitle title="Users" subtitle="review accounts, roles and subscriptions." />
      <ContentTable title="User Accounts" subtitle="Accounts from current seeded data">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}

        {actionError && (
          <div className="alert alert-danger py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
          </div>
        )}

        {editingUserId && (
          <form className="border rounded p-3 mb-3 bg-body" onSubmit={saveUser}>
            <div className="d-flex align-items-center mb-3">
              <div className="flex-grow-1">
                <h6 className="mb-1">Edit {selectedUser?.name}</h6>
                <div className="fs-13px">Update profile details, role and subscription.</div>
              </div>
              <button type="button" className="btn btn-sm btn-default" onClick={cancelEditUser} disabled={isSaving}>
                <i className="fa fa-xmark"></i>
              </button>
            </div>

            <div className="row g-3">
              <div className="col-md-3">
                <label className="form-label">User name</label>
                <input
                  type="text"
                  name="userName"
                  className="form-control"
                  value={formData.userName}
                  onChange={updateFormField}
                  required
                />
              </div>
              <div className="col-md-3">
                <label className="form-label">Email</label>
                <input
                  type="email"
                  name="email"
                  className="form-control"
                  value={formData.email}
                  onChange={updateFormField}
                  required
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">Role</label>
                <select
                  name="roleId"
                  className="form-select"
                  value={formData.roleId}
                  onChange={updateFormField}
                  required
                >
                  {roles.map((role) => (
                    <option key={role.id} value={role.id}>{role.name}</option>
                  ))}
                </select>
              </div>
              <div className="col-md-2">
                <label className="form-label">Subscription</label>
                <select
                  name="subscriptionId"
                  className="form-select"
                  value={formData.subscriptionId}
                  onChange={updateFormField}
                >
                  <option value="">No subscription</option>
                  {subscriptions.map((subscription) => (
                    <option key={subscription.id} value={subscription.id}>{subscription.name}</option>
                  ))}
                </select>
              </div>
              <div className="col-md-2 d-flex align-items-end">
                <button type="submit" className="btn btn-primary w-100" disabled={isSaving}>
                  {isSaving ? (
                    <span className="spinner-border spinner-border-sm"></span>
                  ) : (
                    <>
                      <i className="fa fa-floppy-disk me-2"></i>Save
                    </>
                  )}
                </button>
              </div>
            </div>
          </form>
        )}

        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Role</th>
              <th>Subscription</th>
              <th>Status</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="6" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading users...
                </td>
              </tr>
            )}

            {!isLoading && users.length === 0 && (
              <tr>
                <td colSpan="6" className="text-center py-4">No users found.</td>
              </tr>
            )}

            {!isLoading && users.map((user) => (
              <tr key={user.id}>
                <td className="fw-600 text-body">{user.name}</td>
                <td>{user.email}</td>
                <td>{user.role}</td>
                <td>{user.subscription}</td>
                <td>
                  <span className="badge bg-success bg-opacity-20 text-success">
                    {user.status}
                  </span>
                </td>
                <td className="text-end">
                  <button type="button" className="btn btn-sm btn-default me-1" onClick={() => startEditUser(user)} disabled={deletingUserId === user.id}>
                    <i className="fa fa-user-pen"></i>
                  </button>
                  <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => deleteUser(user)} disabled={deletingUserId === user.id}>
                    {deletingUserId === user.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function mapApiReview(review) {
  return {
    id: review.id,
    user: review.user,
    content: review.content,
    type: review.type,
    rating: review.rating,
    comment: review.comment,
  };
}

function ReviewsPage({ onReviewCountChange }) {
  const [reviews, setReviews] = useState(fallbackReviews);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [deletingReviewId, setDeletingReviewId] = useState(null);

  useEffect(() => {
    let isMounted = true;

    async function loadReviews() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/reviews`);

        if (!response.ok) {
          throw new Error(`Reviews API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          const mappedReviews = data.map(mapApiReview);
          setReviews(mappedReviews);
          onReviewCountChange(mappedReviews.length);
        }
      } catch (error) {
        if (isMounted) {
          setReviews(fallbackReviews);
          onReviewCountChange(fallbackReviews.length);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadReviews();

    return () => {
      isMounted = false;
    };
  }, []);

  const deleteReview = async (review) => {
    const confirmed = window.confirm(`Delete review for "${review.content}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingReviewId(review.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/reviews/${review.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete review returned ${response.status}`);
      }

      setReviews((current) => {
        const nextReviews = current.filter((item) => item.id !== review.id);
        onReviewCountChange(nextReviews.length);
        return nextReviews;
      });
    } catch (error) {
      setActionError("Review was not deleted. Check backend connection and try again.");
    } finally {
      setDeletingReviewId(null);
    }
  };

  return (
    <>
      <PageTitle title="Reviews" subtitle="moderate ratings and user comments." />
      <ContentTable title="Latest Reviews" subtitle="Reviews linked to movies and shows">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}
        {actionError && (
          <div className="alert alert-danger py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
          </div>
        )}

        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>User</th>
              <th>Content</th>
              <th>Type</th>
              <th>Rating</th>
              <th>Comment</th>
              <th className="text-end">Moderation</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="6" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading reviews...
                </td>
              </tr>
            )}

            {!isLoading && reviews.length === 0 && (
              <tr>
                <td colSpan="6" className="text-center py-4">No reviews found.</td>
              </tr>
            )}

            {!isLoading && reviews.map((review) => (
              <tr key={review.id}>
                <td>{review.user}</td>
                <td className="fw-600 text-body">{review.content}</td>
                <td>{review.type}</td>
                <td><Stars value={review.rating} /></td>
                <td>{review.comment}</td>
                <td className="text-end">
                  <button type="button" className="btn btn-sm btn-success me-1" disabled title="Approved">
                    <i className="fa fa-check"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingReviewId === review.id}
                    onClick={() => deleteReview(review)}
                  >
                    {deletingReviewId === review.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function AuditLogsPage() {
  const [auditState, setAuditState] = useState({
    enabled: false,
    message: "Loading MongoDB audit logs...",
    events: [],
  });
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadAuditLogs() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/audit`);
        const data = await response.json();

        if (!response.ok) {
          throw new Error(data?.message || `Audit API returned ${response.status}`);
        }

        if (isMounted) {
          setAuditState({
            enabled: Boolean(data.enabled),
            message: data.message || "MongoDB audit logging status loaded.",
            events: data.events || [],
          });
        }
      } catch (error) {
        if (isMounted) {
          setLoadError(error.message || "MongoDB audit logs are unavailable.");
          setAuditState({
            enabled: false,
            message: "MongoDB is not reachable or backend audit API is unavailable.",
            events: [],
          });
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadAuditLogs();

    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <>
      <PageTitle title="Audit Logs" subtitle="MongoDB-backed user activity events." />

      <div className="row mb-3">
        <div className="col-xl-4">
          <div className="card h-100">
            <div className="card-body">
              <div className="d-flex align-items-center justify-content-between">
                <div>
                  <div className="fs-13px text-body text-opacity-50 mb-1">MongoDB status</div>
                  <h4 className="mb-0">{auditState.enabled ? "Enabled" : "Unavailable"}</h4>
                </div>
                <span className={`badge ${auditState.enabled ? "bg-success" : "bg-warning"} bg-opacity-20 text-body`}>
                  {auditState.enabled ? "Connected" : "Check service"}
                </span>
              </div>
              <p className="text-body text-opacity-75 mt-3 mb-0">{auditState.message}</p>
            </div>
          </div>
        </div>

        <div className="col-xl-4 mt-3 mt-xl-0">
          <div className="card h-100">
            <div className="card-body">
              <div className="fs-13px text-body text-opacity-50 mb-1">Collection</div>
              <h4 className="mb-0">zetflix_logs.audit_events</h4>
              <p className="text-body text-opacity-75 mt-3 mb-0">
                Actions are also mirrored to App_Data/audit.log.
              </p>
            </div>
          </div>
        </div>

        <div className="col-xl-4 mt-3 mt-xl-0">
          <div className="card h-100">
            <div className="card-body">
              <div className="fs-13px text-body text-opacity-50 mb-1">Loaded events</div>
              <h4 className="mb-0">{auditState.events.length}</h4>
              <p className="text-body text-opacity-75 mt-3 mb-0">
                Create a review or subscription action to add a fresh event.
              </p>
            </div>
          </div>
        </div>
      </div>

      <ContentTable title="Recent Audit Events" subtitle="latest records from MongoDB">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}

        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>Time</th>
              <th>User</th>
              <th>Action</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="4" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading audit logs...
                </td>
              </tr>
            )}

            {!isLoading && auditState.events.length === 0 && (
              <tr>
                <td colSpan="4" className="text-center py-4">No MongoDB audit events found.</td>
              </tr>
            )}

            {!isLoading && auditState.events.map((event, index) => (
              <tr key={event.id || `${event.time}-${index}`}>
                <td>{event.time ? new Date(event.time).toLocaleString() : "-"}</td>
                <td>{event.user || "anonymous"}</td>
                <td><span className="badge bg-primary bg-opacity-20 text-body">{event.action}</span></td>
                <td>{event.details}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function GenresPage() {
  const [genres, setGenres] = useState(fallbackGenres);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [deletingGenreId, setDeletingGenreId] = useState(null);
  const [editingGenreId, setEditingGenreId] = useState(null);
  const [genreName, setGenreName] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadGenres() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/genres`);

        if (!response.ok) {
          throw new Error(`Genres API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setGenres(data);
        }
      } catch (error) {
        if (isMounted) {
          setGenres(fallbackGenres);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadGenres();

    return () => {
      isMounted = false;
    };
  }, []);

  const resetForm = () => {
    setGenreName("");
    setEditingGenreId(null);
    setActionError("");
  };

  const startEditGenre = (genre) => {
    setGenreName(genre.name);
    setEditingGenreId(genre.id);
    setActionError("");
  };

  const submitGenre = async (event) => {
    event.preventDefault();

    if (!genreName.trim()) {
      setActionError("Genre name is required.");
      return;
    }

    try {
      setIsSaving(true);
      setActionError("");

      const url = editingGenreId
        ? `${apiBaseUrl}/api/admin/genres/${editingGenreId}`
        : `${apiBaseUrl}/api/admin/genres`;

      const response = await adminFetch(url, {
        method: editingGenreId ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          name: genreName,
        }),
      });

      if (!response.ok) {
        throw new Error(`Save genre returned ${response.status}`);
      }

      const savedGenre = await response.json();

      setGenres((current) => {
        if (editingGenreId) {
          return current
            .map((genre) => genre.id === editingGenreId ? savedGenre : genre)
            .sort((left, right) => left.name.localeCompare(right.name));
        }

        return [...current, savedGenre].sort((left, right) => left.name.localeCompare(right.name));
      });

      resetForm();
    } catch (error) {
      setActionError("Genre was not saved. It may already exist or backend is unavailable.");
    } finally {
      setIsSaving(false);
    }
  };

  const deleteGenre = async (genre) => {
    const confirmed = window.confirm(`Delete "${genre.name}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingGenreId(genre.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/genres/${genre.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete genre returned ${response.status}`);
      }

      setGenres((current) => current.filter((item) => item.id !== genre.id));
      if (editingGenreId === genre.id) {
        resetForm();
      }
    } catch (error) {
      setActionError("Genre was not deleted. It may be used by movies or shows.");
    } finally {
      setDeletingGenreId(null);
    }
  };

  return (
    <>
      <PageTitle title="Genres" subtitle="manage catalog tags used by movies and shows." />

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="mb-3">{editingGenreId ? "Edit Genre" : "Create Genre"}</h5>

          {actionError && (
            <div className="alert alert-danger py-2">
              <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
            </div>
          )}

          <form className="row align-items-end" onSubmit={submitGenre}>
            <div className="col-md-6 mb-2 mb-md-0">
              <label className="form-label">Name</label>
              <input
                className="form-control"
                value={genreName}
                onChange={(event) => setGenreName(event.target.value)}
                placeholder="Genre name"
              />
            </div>
            <div className="col-md-6 d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={isSaving}>
                {isSaving ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="fa fa-save me-2"></i>
                    {editingGenreId ? "Update Genre" : "Save Genre"}
                  </>
                )}
              </button>
              <button type="button" className="btn btn-default" onClick={resetForm} disabled={isSaving}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>

      <ContentTable title="Genre Library" subtitle="Genres loaded from backend">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}
        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="3" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading genres...
                </td>
              </tr>
            )}
            {!isLoading && genres.length === 0 && (
              <tr>
                <td colSpan="3" className="text-center py-4">No genres found.</td>
              </tr>
            )}
            {!isLoading && genres.map((genre) => (
              <tr key={genre.id}>
                <td>{genre.id}</td>
                <td className="fw-600 text-body">{genre.name}</td>
                <td className="text-end">
                  <button
                    type="button"
                    className="btn btn-sm btn-default me-1"
                    onClick={() => startEditGenre(genre)}
                  >
                    <i className="fa fa-pen"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingGenreId === genre.id}
                    onClick={() => deleteGenre(genre)}
                  >
                    {deletingGenreId === genre.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function mapApiSubscription(subscription) {
  return {
    id: subscription.id,
    name: subscription.name,
    price: subscription.price,
    durationInDays: subscription.durationInDays,
    userCount: subscription.userCount,
  };
}

function SubscriptionsPage() {
  const [subscriptions, setSubscriptions] = useState(fallbackSubscriptions);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [deletingSubscriptionId, setDeletingSubscriptionId] = useState(null);
  const [editingSubscriptionId, setEditingSubscriptionId] = useState(null);
  const [formData, setFormData] = useState({
    name: "",
    price: "",
    durationInDays: 30,
  });

  useEffect(() => {
    let isMounted = true;

    async function loadSubscriptions() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/subscriptions`);

        if (!response.ok) {
          throw new Error(`Subscriptions API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setSubscriptions(data.map(mapApiSubscription));
        }
      } catch (error) {
        if (isMounted) {
          setSubscriptions(fallbackSubscriptions);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadSubscriptions();

    return () => {
      isMounted = false;
    };
  }, []);

  function resetSubscriptionForm() {
    setEditingSubscriptionId(null);
    setActionError("");
    setFormData({
      name: "",
      price: "",
      durationInDays: 30,
    });
  }

  function startEditSubscription(subscription) {
    setEditingSubscriptionId(subscription.id);
    setActionError("");
    setFormData({
      name: subscription.name,
      price: subscription.price,
      durationInDays: subscription.durationInDays,
    });
  }

  function updateSubscriptionForm(event) {
    const { name, value } = event.target;
    setFormData((currentFormData) => ({
      ...currentFormData,
      [name]: value,
    }));
  }

  async function submitSubscription(event) {
    event.preventDefault();

    if (!formData.name.trim()) {
      setActionError("Subscription name is required.");
      return;
    }

    try {
      setIsSaving(true);
      setActionError("");

      const url = editingSubscriptionId
        ? `${apiBaseUrl}/api/admin/subscriptions/${editingSubscriptionId}`
        : `${apiBaseUrl}/api/admin/subscriptions`;

      const response = await adminFetch(url, {
        method: editingSubscriptionId ? "PUT" : "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: formData.name,
          price: Number(formData.price),
          durationInDays: Number(formData.durationInDays),
        }),
      });

      if (!response.ok) {
        throw new Error(`Save subscription returned ${response.status}`);
      }

      const savedSubscription = mapApiSubscription(await response.json());

      setSubscriptions((currentSubscriptions) => {
        if (editingSubscriptionId) {
          return currentSubscriptions.map((subscription) => (
            subscription.id === savedSubscription.id ? savedSubscription : subscription
          ));
        }

        return [...currentSubscriptions, savedSubscription];
      });

      resetSubscriptionForm();
    } catch (error) {
      setActionError("Subscription was not saved. Check price, duration and backend API.");
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteSubscription(subscription) {
    const confirmed = window.confirm(`Delete "${subscription.name}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingSubscriptionId(subscription.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/subscriptions/${subscription.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete subscription returned ${response.status}`);
      }

      setSubscriptions((currentSubscriptions) => currentSubscriptions.filter((item) => item.id !== subscription.id));

      if (editingSubscriptionId === subscription.id) {
        resetSubscriptionForm();
      }
    } catch (error) {
      setActionError("Subscription was not deleted. It may still be assigned to users.");
    } finally {
      setDeletingSubscriptionId(null);
    }
  }

  return (
    <>
      <PageTitle title="Subscriptions" subtitle="manage paid access plans for users." />

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="mb-3">{editingSubscriptionId ? "Edit Subscription" : "Create Subscription"}</h5>

          {actionError && (
            <div className="alert alert-danger py-2">
              <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
            </div>
          )}

          <form className="row align-items-end" onSubmit={submitSubscription}>
            <div className="col-md-4 mb-2 mb-md-0">
              <label className="form-label">Name</label>
              <input
                className="form-control"
                name="name"
                value={formData.name}
                onChange={updateSubscriptionForm}
                placeholder="Subscription name"
                required
              />
            </div>
            <div className="col-md-3 mb-2 mb-md-0">
              <label className="form-label">Price</label>
              <input
                className="form-control"
                name="price"
                type="number"
                min="0"
                step="0.01"
                value={formData.price}
                onChange={updateSubscriptionForm}
                placeholder="599"
                required
              />
            </div>
            <div className="col-md-2 mb-2 mb-md-0">
              <label className="form-label">Days</label>
              <input
                className="form-control"
                name="durationInDays"
                type="number"
                min="1"
                value={formData.durationInDays}
                onChange={updateSubscriptionForm}
                required
              />
            </div>
            <div className="col-md-3 d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={isSaving}>
                {isSaving ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="fa fa-save me-2"></i>
                    {editingSubscriptionId ? "Update" : "Save"}
                  </>
                )}
              </button>
              <button type="button" className="btn btn-default" onClick={resetSubscriptionForm} disabled={isSaving}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>

      <ContentTable title="Subscription Plans" subtitle="Plans loaded from backend">
        {loadError && (
          <div className="alert alert-warning py-2 mb-3">
            <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
          </div>
        )}
        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>Plan</th>
              <th>Price</th>
              <th>Duration</th>
              <th>Users</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="5" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading subscriptions...
                </td>
              </tr>
            )}
            {!isLoading && subscriptions.length === 0 && (
              <tr>
                <td colSpan="5" className="text-center py-4">No subscriptions found.</td>
              </tr>
            )}
            {!isLoading && subscriptions.map((subscription) => (
              <tr key={subscription.id}>
                <td className="fw-600 text-body">{subscription.name}</td>
                <td>{Number(subscription.price).toLocaleString("en-US")} RUB</td>
                <td>{subscription.durationInDays} days</td>
                <td>
                  <span className="badge bg-primary bg-opacity-20 text-primary">
                    {subscription.userCount}
                  </span>
                </td>
                <td className="text-end">
                  <button
                    type="button"
                    className="btn btn-sm btn-default me-1"
                    onClick={() => startEditSubscription(subscription)}
                  >
                    <i className="fa fa-pen"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingSubscriptionId === subscription.id || subscription.userCount > 0}
                    onClick={() => deleteSubscription(subscription)}
                    title={subscription.userCount > 0 ? "Plan is assigned to users" : "Delete subscription"}
                  >
                    {deletingSubscriptionId === subscription.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function mapApiMediaFile(file) {
  return {
    fileName: file.fileName,
    url: file.url,
    contentType: file.contentType,
    size: file.size,
    lastModifiedUtc: file.lastModifiedUtc,
  };
}

function formatFileSize(size) {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}

function MediaPage() {
  const [files, setFiles] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const [deletingFileName, setDeletingFileName] = useState("");
  const [copiedUrl, setCopiedUrl] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadFiles() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/media`);

        if (!response.ok) {
          throw new Error(`Media API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setFiles(data.map(mapApiMediaFile));
        }
      } catch (error) {
        if (isMounted) {
          setFiles([]);
          setLoadError(getApiLoadErrorMessage(error, "media files could not be loaded"));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadFiles();

    return () => {
      isMounted = false;
    };
  }, []);

  async function uploadMedia(event) {
    event.preventDefault();

    if (!selectedFile) {
      setActionError("Choose a file first.");
      return;
    }

    try {
      setIsUploading(true);
      setActionError("");

      const payload = new FormData();
      payload.append("file", selectedFile);

      const response = await adminFetch(`${apiBaseUrl}/api/admin/media`, {
        method: "POST",
        body: payload,
      });

      if (!response.ok) {
        throw new Error(`Upload media returned ${response.status}`);
      }

      const uploadedFile = mapApiMediaFile(await response.json());
      setFiles((currentFiles) => [uploadedFile, ...currentFiles]);
      setSelectedFile(null);
      event.target.reset();
    } catch (error) {
      setActionError("File was not uploaded. Use jpg, png, webp, gif, mp4 or webm up to 50 MB.");
    } finally {
      setIsUploading(false);
    }
  }

  async function copyMediaUrl(url) {
    try {
      await navigator.clipboard.writeText(url);
      setCopiedUrl(url);
      window.setTimeout(() => setCopiedUrl(""), 1800);
    } catch (error) {
      setActionError("Could not copy URL. Select it manually from the file card.");
    }
  }

  async function deleteMediaFile(file) {
    const confirmed = window.confirm(`Delete "${file.fileName}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingFileName(file.fileName);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/media/${encodeURIComponent(file.fileName)}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete media returned ${response.status}`);
      }

      setFiles((currentFiles) => currentFiles.filter((item) => item.fileName !== file.fileName));
    } catch (error) {
      setActionError("File was not deleted. Check the backend API and try again.");
    } finally {
      setDeletingFileName("");
    }
  }

  return (
    <>
      <PageTitle title="Media Library" subtitle="upload posters and media used by catalog items." />

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="mb-3">Upload Media</h5>

          {actionError && (
            <div className="alert alert-danger py-2">
              <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
            </div>
          )}

          <form className="row align-items-end" onSubmit={uploadMedia}>
            <div className="col-md-8 mb-2 mb-md-0">
              <label className="form-label">File</label>
              <input
                className="form-control"
                type="file"
                accept=".jpg,.jpeg,.png,.webp,.gif,.mp4,.webm"
                onChange={(event) => setSelectedFile(event.target.files?.[0] || null)}
              />
            </div>
            <div className="col-md-4">
              <button type="submit" className="btn btn-primary w-100" disabled={isUploading}>
                {isUploading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Uploading...
                  </>
                ) : (
                  <>
                    <i className="fa fa-upload me-2"></i>
                    Upload
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="d-flex align-items-center mb-3">
            <div className="flex-grow-1">
              <h5 className="mb-1">Uploaded Files</h5>
              <div className="fs-13px">Copy a file URL and paste it into movie or show Poster URL.</div>
            </div>
            <span className="badge bg-primary bg-opacity-20 text-primary">{files.length}</span>
          </div>

          {loadError && (
            <div className="alert alert-warning py-2 mb-3">
              <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
            </div>
          )}

          {isLoading && (
            <div className="text-center py-4">
              <span className="spinner-border spinner-border-sm me-2"></span>Loading media...
            </div>
          )}

          {!isLoading && files.length === 0 && (
            <div className="text-center py-4">No media files uploaded yet.</div>
          )}

          {!isLoading && files.length > 0 && (
            <div className="studio-media-grid">
              {files.map((file) => (
                <div key={file.fileName} className="studio-media-card">
                  <div className="studio-media-preview">
                    {file.contentType.startsWith("image/") ? (
                      <img src={backendAsset(file.url)} alt={file.fileName} />
                    ) : (
                      <video src={backendAsset(file.url)} muted controls />
                    )}
                  </div>
                  <div className="p-3">
                    <div className="fw-600 text-body text-truncate" title={file.fileName}>{file.fileName}</div>
                    <div className="fs-13px mb-2">{formatFileSize(file.size)}</div>
                    <div className="input-group input-group-sm mb-2">
                      <input className="form-control" value={file.url} readOnly />
                      <button type="button" className="btn btn-default" onClick={() => copyMediaUrl(file.url)}>
                        <i className={`fa ${copiedUrl === file.url ? "fa-check" : "fa-copy"}`}></i>
                      </button>
                    </div>
                    <button
                      type="button"
                      className="btn btn-sm btn-danger w-100"
                      disabled={deletingFileName === file.fileName}
                      onClick={() => deleteMediaFile(file)}
                    >
                      {deletingFileName === file.fileName ? (
                        <span className="spinner-border spinner-border-sm"></span>
                      ) : (
                        <>
                          <i className="fa fa-trash me-2"></i>
                          Delete
                        </>
                      )}
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  );
}

function mapApiEpisodeLibrary(episode) {
  return {
    id: episode.id,
    title: episode.title,
    description: episode.description,
    episodeNumber: episode.episodeNumber,
    videoUrl: episode.videoUrl,
    seasonId: episode.seasonId,
    seasonNumber: episode.seasonNumber,
    showId: episode.showId,
    showTitle: episode.showTitle,
  };
}

function EpisodesPage() {
  const [episodes, setEpisodes] = useState(fallbackEpisodeLibrary);
  const [shows, setShows] = useState(fallbackShows);
  const [seasons, setSeasons] = useState([]);
  const [selectedShowId, setSelectedShowId] = useState(fallbackShows[0]?.id || "");
  const [selectedSeasonId, setSelectedSeasonId] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSeasonLoading, setIsSeasonLoading] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [deletingEpisodeId, setDeletingEpisodeId] = useState(null);
  const [editingEpisodeId, setEditingEpisodeId] = useState(null);
  const [formData, setFormData] = useState({
    title: "",
    episodeNumber: 1,
    videoUrl: "",
    description: "",
  });

  useEffect(() => {
    let isMounted = true;

    async function loadEpisodeLibrary() {
      try {
        setIsLoading(true);
        setLoadError("");

        const [episodesResponse, showsResponse] = await Promise.all([
          adminFetch(`${apiBaseUrl}/api/admin/episodes`),
          adminFetch(`${apiBaseUrl}/api/admin/shows`),
        ]);

        if (!episodesResponse.ok) {
          throw new Error(`Episodes API returned ${episodesResponse.status}`);
        }

        if (!showsResponse.ok) {
          throw new Error(`Shows API returned ${showsResponse.status}`);
        }

        const [episodesData, showsData] = await Promise.all([
          episodesResponse.json(),
          showsResponse.json(),
        ]);

        if (isMounted) {
          const mappedShows = showsData.map(mapApiShow);
          setEpisodes(episodesData.map(mapApiEpisodeLibrary));
          setShows(mappedShows);
          setSelectedShowId(mappedShows[0]?.id || "");
        }
      } catch (error) {
        if (isMounted) {
          setEpisodes(fallbackEpisodeLibrary);
          setShows(fallbackShows);
          setSelectedShowId(fallbackShows[0]?.id || "");
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadEpisodeLibrary();

    return () => {
      isMounted = false;
    };
  }, []);

  useEffect(() => {
    let isMounted = true;

    async function loadShowSeasons() {
      if (!selectedShowId) {
        setSeasons([]);
        setSelectedSeasonId("");
        return;
      }

      try {
        setIsSeasonLoading(true);
        setActionError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/shows/${selectedShowId}/seasons`);

        if (!response.ok) {
          throw new Error(`Seasons API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setSeasons(data);
          setSelectedSeasonId(data[0]?.id || "");
          setEditingEpisodeId(null);
          setFormData({
            title: "",
            episodeNumber: data[0] ? nextEpisodeNumber(episodes, data[0].id) : 1,
            videoUrl: "",
            description: "",
          });
        }
      } catch (error) {
        if (isMounted) {
          const fallbackSeasons = fallbackEpisodeLibrary
            .filter((episode) => episode.showId === Number(selectedShowId))
            .reduce((items, episode) => (
              items.some((season) => season.id === episode.seasonId)
                ? items
                : [...items, { id: episode.seasonId, number: episode.seasonNumber, showId: episode.showId, episodesCount: 0 }]
            ), []);

          setSeasons(fallbackSeasons);
          setSelectedSeasonId(fallbackSeasons[0]?.id || "");
        }
      } finally {
        if (isMounted) {
          setIsSeasonLoading(false);
        }
      }
    }

    loadShowSeasons();

    return () => {
      isMounted = false;
    };
  }, [selectedShowId]);

  function nextEpisodeNumber(items, seasonId) {
    const seasonEpisodes = items.filter((episode) => episode.seasonId === Number(seasonId));
    return seasonEpisodes.length ? Math.max(...seasonEpisodes.map((episode) => episode.episodeNumber)) + 1 : 1;
  }

  function resetEpisodeForm() {
    setEditingEpisodeId(null);
    setActionError("");
    setFormData({
      title: "",
      episodeNumber: nextEpisodeNumber(episodes, selectedSeasonId),
      videoUrl: "",
      description: "",
    });
  }

  function updateEpisodeForm(event) {
    const { name, value } = event.target;
    setFormData((currentFormData) => ({
      ...currentFormData,
      [name]: value,
    }));
  }

  function selectSeason(event) {
    const seasonId = event.target.value;
    setSelectedSeasonId(seasonId);
    setEditingEpisodeId(null);
    setActionError("");
    setFormData({
      title: "",
      episodeNumber: nextEpisodeNumber(episodes, seasonId),
      videoUrl: "",
      description: "",
    });
  }

  function startEditEpisode(episode) {
    setSelectedShowId(episode.showId);
    setSelectedSeasonId(episode.seasonId);
    setEditingEpisodeId(episode.id);
    setActionError("");
    setFormData({
      title: episode.title,
      episodeNumber: episode.episodeNumber,
      videoUrl: episode.videoUrl,
      description: episode.description,
    });
  }

  async function submitEpisode(event) {
    event.preventDefault();

    if (!selectedSeasonId) {
      setActionError("Choose a season before saving an episode.");
      return;
    }

    try {
      setIsSaving(true);
      setActionError("");

      const url = editingEpisodeId
        ? `${apiBaseUrl}/api/admin/seasons/${selectedSeasonId}/episodes/${editingEpisodeId}`
        : `${apiBaseUrl}/api/admin/seasons/${selectedSeasonId}/episodes`;

      const response = await adminFetch(url, {
        method: editingEpisodeId ? "PUT" : "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          title: formData.title,
          description: formData.description,
          episodeNumber: Number(formData.episodeNumber),
          videoUrl: formData.videoUrl,
        }),
      });

      if (!response.ok) {
        throw new Error(`Save episode returned ${response.status}`);
      }

      const savedEpisode = await response.json();
      const selectedShow = shows.find((show) => show.id === Number(selectedShowId));
      const selectedSeason = seasons.find((season) => season.id === Number(selectedSeasonId));
      const libraryEpisode = {
        ...savedEpisode,
        showId: Number(selectedShowId),
        showTitle: selectedShow?.title || "Unknown show",
        seasonNumber: selectedSeason?.number || 0,
      };

      setEpisodes((currentEpisodes) => {
        if (editingEpisodeId) {
          return currentEpisodes
            .map((episode) => episode.id === editingEpisodeId ? libraryEpisode : episode)
            .sort(sortEpisodeLibrary);
        }

        return [...currentEpisodes, libraryEpisode].sort(sortEpisodeLibrary);
      });

      resetEpisodeForm();
    } catch (error) {
      setActionError("Episode was not saved. It may duplicate a number or backend is unavailable.");
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteEpisode(episode) {
    const confirmed = window.confirm(`Delete "${episode.title}"?`);

    if (!confirmed) {
      return;
    }

    try {
      setDeletingEpisodeId(episode.id);
      setActionError("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/seasons/${episode.seasonId}/episodes/${episode.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error(`Delete episode returned ${response.status}`);
      }

      setEpisodes((currentEpisodes) => currentEpisodes.filter((item) => item.id !== episode.id));

      if (editingEpisodeId === episode.id) {
        resetEpisodeForm();
      }
    } catch (error) {
      setActionError("Episode was not deleted. Check backend connection and try again.");
    } finally {
      setDeletingEpisodeId(null);
    }
  }

  function sortEpisodeLibrary(left, right) {
    return left.showTitle.localeCompare(right.showTitle) ||
      left.seasonNumber - right.seasonNumber ||
      left.episodeNumber - right.episodeNumber;
  }

  const visibleEpisodes = episodes.filter((episode) => {
    if (selectedShowId && episode.showId !== Number(selectedShowId)) return false;
    if (selectedSeasonId && episode.seasonId !== Number(selectedSeasonId)) return false;
    return true;
  });

  return (
    <>
      <PageTitle title="Episodes" subtitle="manage playable entries across all shows." />

      {loadError && (
        <div className="alert alert-warning py-2">
          <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
        </div>
      )}

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="mb-3">{editingEpisodeId ? "Edit Episode" : "Create Episode"}</h5>

          {actionError && (
            <div className="alert alert-danger py-2">
              <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
            </div>
          )}

          <form onSubmit={submitEpisode}>
            <div className="row">
              <div className="col-md-4 mb-3">
                <label className="form-label">Show</label>
                <select
                  className="form-select"
                  value={selectedShowId}
                  onChange={(event) => setSelectedShowId(event.target.value)}
                  disabled={isSeasonLoading || isSaving}
                >
                  {shows.map((show) => (
                    <option key={show.id} value={show.id}>{show.title}</option>
                  ))}
                </select>
              </div>
              <div className="col-md-3 mb-3">
                <label className="form-label">Season</label>
                <select
                  className="form-select"
                  value={selectedSeasonId}
                  onChange={selectSeason}
                  disabled={isSeasonLoading || isSaving || seasons.length === 0}
                >
                  {seasons.map((season) => (
                    <option key={season.id} value={season.id}>Season {season.number}</option>
                  ))}
                </select>
              </div>
              <div className="col-md-3 mb-3">
                <label className="form-label">Title</label>
                <input
                  className="form-control"
                  name="title"
                  value={formData.title}
                  onChange={updateEpisodeForm}
                  required
                />
              </div>
              <div className="col-md-2 mb-3">
                <label className="form-label">Episode #</label>
                <input
                  className="form-control"
                  name="episodeNumber"
                  type="number"
                  min="1"
                  value={formData.episodeNumber}
                  onChange={updateEpisodeForm}
                  required
                />
              </div>
            </div>

            <div className="row">
              <div className="col-md-5 mb-3">
                <label className="form-label">Video URL</label>
                <input
                  className="form-control"
                  name="videoUrl"
                  value={formData.videoUrl}
                  onChange={updateEpisodeForm}
                  placeholder="/videos/episode.mp4"
                  required
                />
              </div>
              <div className="col-md-7 mb-3">
                <label className="form-label">Description</label>
                <input
                  className="form-control"
                  name="description"
                  value={formData.description}
                  onChange={updateEpisodeForm}
                  required
                />
              </div>
            </div>

            <div className="d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={isSaving || !selectedSeasonId}>
                {isSaving ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="fa fa-save me-2"></i>
                    {editingEpisodeId ? "Update Episode" : "Save Episode"}
                  </>
                )}
              </button>
              <button type="button" className="btn btn-default" onClick={resetEpisodeForm} disabled={isSaving}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>

      <ContentTable title="Episode Library" subtitle="Episodes grouped by selected show and season">
        <table className="table table-hover align-middle mb-0">
          <thead>
            <tr>
              <th>Episode</th>
              <th>Show</th>
              <th>Season</th>
              <th>Video</th>
              <th className="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan="5" className="text-center py-4">
                  <span className="spinner-border spinner-border-sm me-2"></span>Loading episodes...
                </td>
              </tr>
            )}
            {!isLoading && visibleEpisodes.length === 0 && (
              <tr>
                <td colSpan="5" className="text-center py-4">No episodes found for this selection.</td>
              </tr>
            )}
            {!isLoading && visibleEpisodes.map((episode) => (
              <tr key={episode.id}>
                <td>
                  <div className="fw-600 text-body">#{episode.episodeNumber} {episode.title}</div>
                  <div className="fs-13px text-truncate" style={{ maxWidth: 360 }}>{episode.description}</div>
                </td>
                <td>{episode.showTitle}</td>
                <td>Season {episode.seasonNumber}</td>
                <td className="text-truncate" style={{ maxWidth: 220 }}>{episode.videoUrl}</td>
                <td className="text-end">
                  <button type="button" className="btn btn-sm btn-default me-1" onClick={() => startEditEpisode(episode)}>
                    <i className="fa fa-pen"></i>
                  </button>
                  <button
                    type="button"
                    className="btn btn-sm btn-danger"
                    disabled={deletingEpisodeId === episode.id}
                    onClick={() => deleteEpisode(episode)}
                  >
                    {deletingEpisodeId === episode.id ? (
                      <span className="spinner-border spinner-border-sm"></span>
                    ) : (
                      <i className="fa fa-trash"></i>
                    )}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </ContentTable>
    </>
  );
}

function AnalyticsPage() {
  const [analytics, setAnalytics] = useState(fallbackAnalytics);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadAnalytics() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/analytics`);

        if (!response.ok) {
          throw new Error(`Analytics API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setAnalytics(data);
        }
      } catch (error) {
        if (isMounted) {
          setAnalytics(fallbackAnalytics);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadAnalytics();

    return () => {
      isMounted = false;
    };
  }, []);

  const maxGenreCount = Math.max(...analytics.genres.map((genre) => genre.movieCount + genre.showCount), 1);
  const maxRatingCount = Math.max(...analytics.ratings.map((rating) => rating.count), 1);
  const roles = analytics.roles || [];
  const moderation = analytics.moderation || fallbackAnalytics.moderation;
  const maxRoleCount = Math.max(...roles.map((role) => role.userCount), 1);

  return (
    <>
      <PageTitle title="Analytics" subtitle="catalog, audience and subscription performance." />

      {loadError && (
        <div className="alert alert-warning py-2">
          <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
        </div>
      )}

      {isLoading && (
        <div className="card mb-3">
          <div className="card-body text-center py-4">
            <span className="spinner-border spinner-border-sm me-2"></span>Loading analytics...
          </div>
        </div>
      )}

      {!isLoading && (
        <>
          <div className="row">
            {analytics.summary.map((metric) => (
              <div className="col-xl col-md-4 col-sm-6 mb-3" key={metric.label}>
                <div className="card h-100">
                  <div className="card-body">
                    <div className="fs-13px mb-2">{metric.label}</div>
                    <h3 className="mb-1">
                      {metric.label === "Revenue"
                        ? `${Number(metric.value).toLocaleString("en-US")} RUB`
                        : Number(metric.value).toLocaleString("en-US")}
                    </h3>
                    <div className="text-success fw-600 fs-13px">
                      <i className="fa fa-caret-up me-1"></i>{metric.hint}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="row">
            <div className="col-xl-6 mb-3">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="mb-1">Genre Breakdown</h5>
                  <div className="fs-13px mb-4">Movies and shows assigned to each genre</div>

                  {analytics.genres.map((genre) => {
                    const total = genre.movieCount + genre.showCount;

                    return (
                      <div className="mb-3" key={genre.name}>
                        <div className="d-flex align-items-center mb-1">
                          <div className="flex-grow-1 fw-600 text-body">{genre.name}</div>
                          <div className="fs-13px">{total} items</div>
                        </div>
                        <div className="progress studio-analytics-progress">
                          <div className="progress-bar bg-primary" style={{ width: `${(genre.movieCount / maxGenreCount) * 100}%` }}></div>
                          <div className="progress-bar bg-teal" style={{ width: `${(genre.showCount / maxGenreCount) * 100}%` }}></div>
                        </div>
                        <div className="fs-12px mt-1">Movies: {genre.movieCount} / Shows: {genre.showCount}</div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>

            <div className="col-xl-6 mb-3">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="mb-1">Rating Distribution</h5>
                  <div className="fs-13px mb-4">Review volume by star rating</div>

                  {[5, 4, 3, 2, 1].map((rating) => {
                    const ratingItem = analytics.ratings.find((item) => item.rating === rating);
                    const count = ratingItem?.count || 0;

                    return (
                      <div className="d-flex align-items-center mb-3" key={rating}>
                        <div className="w-80px text-warning">
                          {rating}<i className="fa fa-star ms-1"></i>
                        </div>
                        <div className="progress studio-analytics-progress flex-grow-1">
                          <div className="progress-bar bg-warning" style={{ width: `${(count / maxRatingCount) * 100}%` }}></div>
                        </div>
                        <div className="w-50px text-end fw-600 text-body">{count}</div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-xl-6 mb-3">
              <ContentTable title="Subscription Revenue" subtitle="Revenue grouped by plan">
                <table className="table table-hover align-middle mb-0">
                  <thead>
                    <tr>
                      <th>Plan</th>
                      <th>Price</th>
                      <th>Users</th>
                      <th className="text-end">Revenue</th>
                    </tr>
                  </thead>
                  <tbody>
                    {analytics.subscriptions.map((subscription) => (
                      <tr key={subscription.name}>
                        <td className="fw-600 text-body">{subscription.name}</td>
                        <td>{Number(subscription.price).toLocaleString("en-US")} RUB</td>
                        <td>{subscription.userCount}</td>
                        <td className="text-end">{Number(subscription.revenue).toLocaleString("en-US")} RUB</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </ContentTable>
            </div>

            <div className="col-xl-6 mb-3">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="mb-1">Content Health</h5>
                  <div className="fs-13px mb-4">Readiness checks for the public catalog</div>

                  {analytics.contentHealth.map((item) => {
                    const percent = item.total > 0 ? Math.round((item.ready / item.total) * 100) : 0;

                    return (
                      <div className="mb-3" key={item.label}>
                        <div className="d-flex align-items-center mb-1">
                          <div className="flex-grow-1 fw-600 text-body">{item.label}</div>
                          <div className="fs-13px">{item.ready}/{item.total}</div>
                        </div>
                        <div className="progress studio-analytics-progress">
                          <div className={`progress-bar ${percent >= 70 ? "bg-success" : "bg-warning"}`} style={{ width: `${percent}%` }}></div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-xl-6 mb-3">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="mb-1">Audience Roles</h5>
                  <div className="fs-13px mb-4">Current access levels assigned to users</div>

                  {roles.map((role) => (
                    <div className="mb-3" key={role.role}>
                      <div className="d-flex align-items-center mb-1">
                        <div className="flex-grow-1 fw-600 text-body">{role.role}</div>
                        <div className="fs-13px">{role.userCount} users</div>
                      </div>
                      <div className="progress studio-analytics-progress">
                        <div className="progress-bar bg-info" style={{ width: `${(role.userCount / maxRoleCount) * 100}%` }}></div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            <div className="col-xl-6 mb-3">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="mb-1">Moderation Snapshot</h5>
                  <div className="fs-13px mb-4">Review workload for admins and moderators</div>
                  <div className="row text-center">
                    <div className="col-4">
                      <div className="fs-13px">Reviews</div>
                      <h3 className="mb-0">{moderation.totalReviews}</h3>
                    </div>
                    <div className="col-4">
                      <div className="fs-13px">Low rating</div>
                      <h3 className="mb-0 text-warning">{moderation.lowRatingReviews}</h3>
                    </div>
                    <div className="col-4">
                      <div className="fs-13px">With comments</div>
                      <h3 className="mb-0 text-success">{moderation.reviewsWithComments}</h3>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </>
  );
}

function SettingsPage() {
  const [settings, setSettings] = useState(fallbackSettings);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [savedMessage, setSavedMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadSettings() {
      try {
        setIsLoading(true);
        setLoadError("");

        const response = await adminFetch(`${apiBaseUrl}/api/admin/settings`);

        if (!response.ok) {
          throw new Error(`Settings API returned ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setSettings(data);
        }
      } catch (error) {
        if (isMounted) {
          setSettings(fallbackSettings);
          setLoadError(getApiLoadErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadSettings();

    return () => {
      isMounted = false;
    };
  }, []);

  function updateSettingsField(event) {
    const { name, value, checked, type } = event.target;
    setSettings((currentSettings) => ({
      ...currentSettings,
      [name]: type === "checkbox" ? checked : value,
    }));
  }

  async function saveSettings(event) {
    event.preventDefault();

    try {
      setIsSaving(true);
      setActionError("");
      setSavedMessage("");

      const response = await adminFetch(`${apiBaseUrl}/api/admin/settings`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(settings),
      });

      if (!response.ok) {
        throw new Error(`Save settings returned ${response.status}`);
      }

      const savedSettings = await response.json();
      setSettings(savedSettings);
      setSavedMessage("Settings saved.");
      window.setTimeout(() => setSavedMessage(""), 2200);
    } catch (error) {
      setActionError("Settings were not saved. Check required fields and backend API.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <>
      <PageTitle title="Settings" subtitle="configure Zetflix platform defaults." />

      {loadError && (
        <div className="alert alert-warning py-2">
          <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
        </div>
      )}

      {isLoading ? (
        <div className="card">
          <div className="card-body text-center py-4">
            <span className="spinner-border spinner-border-sm me-2"></span>Loading settings...
          </div>
        </div>
      ) : (
        <div className="row">
          <div className="col-xl-8 mb-3">
            <div className="card">
              <div className="card-body">
                <h5 className="mb-1">Platform Settings</h5>
                <div className="fs-13px mb-4">These values are saved by the backend and can be reused on the public site.</div>

                {actionError && (
                  <div className="alert alert-danger py-2">
                    <i className="fa fa-triangle-exclamation me-2"></i>{actionError}
                  </div>
                )}

                {savedMessage && (
                  <div className="alert alert-success py-2">
                    <i className="fa fa-check me-2"></i>{savedMessage}
                  </div>
                )}

                <form onSubmit={saveSettings}>
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Site name</label>
                      <input
                        className="form-control"
                        name="siteName"
                        value={settings.siteName}
                        onChange={updateSettingsField}
                        required
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Support email</label>
                      <input
                        className="form-control"
                        name="supportEmail"
                        type="email"
                        value={settings.supportEmail}
                        onChange={updateSettingsField}
                        required
                      />
                    </div>
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Hero title</label>
                    <input
                      className="form-control"
                      name="heroTitle"
                      value={settings.heroTitle}
                      onChange={updateSettingsField}
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Hero subtitle</label>
                    <textarea
                      className="form-control"
                      name="heroSubtitle"
                      rows="3"
                      value={settings.heroSubtitle}
                      onChange={updateSettingsField}
                    ></textarea>
                  </div>

                  <div className="d-flex flex-wrap gap-4 mb-4">
                    <div className="form-check form-switch">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        role="switch"
                        name="allowRegistration"
                        checked={settings.allowRegistration}
                        onChange={updateSettingsField}
                        id="allowRegistration"
                      />
                      <label className="form-check-label" htmlFor="allowRegistration">Allow registration</label>
                    </div>
                    <div className="form-check form-switch">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        role="switch"
                        name="maintenanceMode"
                        checked={settings.maintenanceMode}
                        onChange={updateSettingsField}
                        id="maintenanceMode"
                      />
                      <label className="form-check-label" htmlFor="maintenanceMode">Maintenance mode</label>
                    </div>
                  </div>

                  <button type="submit" className="btn btn-primary" disabled={isSaving}>
                    {isSaving ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Saving...
                      </>
                    ) : (
                      <>
                        <i className="fa fa-floppy-disk me-2"></i>
                        Save Settings
                      </>
                    )}
                  </button>
                </form>
              </div>
            </div>
          </div>

          <div className="col-xl-4 mb-3">
            <div className="card h-100">
              <div className="card-body">
                <h5 className="mb-1">Live Preview</h5>
                <div className="fs-13px mb-4">Admin-side preview of public copy</div>

                <div className="studio-settings-preview">
                  <div className="fs-13px text-uppercase text-white text-opacity-60 mb-2">{settings.siteName}</div>
                  <h3 className="text-white mb-3">{settings.heroTitle}</h3>
                  <p className="text-white text-opacity-75 mb-4">{settings.heroSubtitle}</p>
                  <div className="d-flex gap-2 flex-wrap">
                    <span className={`badge ${settings.allowRegistration ? "bg-success" : "bg-secondary"}`}>
                      Registration {settings.allowRegistration ? "open" : "closed"}
                    </span>
                    <span className={`badge ${settings.maintenanceMode ? "bg-warning text-black" : "bg-primary"}`}>
                      {settings.maintenanceMode ? "Maintenance" : "Online"}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function AdminLoginPage({ onLogin }) {
  const [formData, setFormData] = useState({
    email: "admin@zetflix.com",
    password: "admin123",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  function updateField(event) {
    const { name, value } = event.target;
    setFormData((currentFormData) => ({
      ...currentFormData,
      [name]: value,
    }));
  }

  async function submitLogin(event) {
    event.preventDefault();

    try {
      setIsLoading(true);
      setError("");

      const response = await window.fetch(`${apiBaseUrl}/api/admin/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        if (response.status === 401) {
          setError("Invalid admin credentials. Use admin@zetflix.com / admin123 or another user with the Admin role.");
          return;
        }

        setError(`Backend answered with ${response.status}. Check the backend console for details.`);
        return;
      }

      const session = await response.json();
      localStorage.setItem(adminSessionStorageKey, JSON.stringify(session));
      onLogin(session);
    } catch (error) {
      setError(`Backend is not reachable at ${apiBaseUrl}. Start it with: dotnet run --project Zetflix/Zetflix.csproj --launch-profile http`);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="studio-login-shell">
      <div className="studio-login-card">
        <div className="d-flex align-items-center mb-4">
          <div className="studio-login-logo me-3">Z</div>
          <div>
            <h1 className="h3 mb-1">Zetflix Studio</h1>
            <div className="fs-13px">Admin access</div>
          </div>
        </div>

        {error && (
          <div className="alert alert-danger py-2">
            <i className="fa fa-triangle-exclamation me-2"></i>{error}
          </div>
        )}

        <div className="studio-login-hint mb-4">
          <div className="fw-600 mb-1">Backend URL</div>
          <code>{apiBaseUrl}</code>
        </div>

        <form onSubmit={submitLogin}>
          <div className="mb-3">
            <label className="form-label">Email</label>
            <input
              className="form-control"
              name="email"
              type="email"
              value={formData.email}
              onChange={updateField}
              required
            />
          </div>
          <div className="mb-4">
            <label className="form-label">Password</label>
            <input
              className="form-control"
              name="password"
              type="password"
              value={formData.password}
              onChange={updateField}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary w-100" disabled={isLoading}>
            {isLoading ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>
                Signing in...
              </>
            ) : (
              <>
                <i className="fa fa-lock me-2"></i>
                Sign In
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
}

function PlaceholderPage({ title }) {
  return (
    <>
      <PageTitle title={title} subtitle="this admin section is not enabled." />
      <div className="card">
        <div className="card-body">
          <h5 className="mb-1">{title}</h5>
          <div className="fs-13px mb-4">All primary Zetflix workflows are already connected in the sidebar.</div>
          <button type="button" className="btn btn-primary" onClick={() => window.location.reload()}>Reload dashboard</button>
        </div>
      </div>
    </>
  );
}

function DashboardPage({ onViewRevenue }) {
  const [stats, setStats] = useState(fallbackDashboardStats);
  const [loadError, setLoadError] = useState("");
  const [isRefreshing, setIsRefreshing] = useState(false);

  async function loadDashboard() {
    try {
      setIsRefreshing(true);
      setLoadError("");
      const response = await adminFetch(`${apiBaseUrl}/api/admin/dashboard?ts=${Date.now()}`, {
        headers: { "Cache-Control": "no-cache" },
      });

      if (!response.ok) {
        throw new Error(`Dashboard API returned ${response.status}`);
      }

      const data = await response.json();

      setStats(data);
    } catch (error) {
      setStats(fallbackDashboardStats);
      setLoadError(getApiLoadErrorMessage(error));
    } finally {
      setIsRefreshing(false);
    }
  }

  useEffect(() => {
    loadDashboard();

    const intervalId = window.setInterval(loadDashboard, 15000);
    return () => window.clearInterval(intervalId);
  }, []);

  const dashboardCards = [
    { title: "Movies", value: stats.movieCount.toLocaleString("en-US"), trend: `${stats.reviewCount} reviews`, icon: "img/icon/order.svg", color: "orange", width: `${Math.min(stats.movieCount * 18, 95)}%`, up: true },
    { title: "Shows", value: stats.showCount.toLocaleString("en-US"), trend: `${stats.episodeCount} episodes`, icon: "img/icon/email.svg", color: "pink", width: `${Math.min(stats.showCount * 22, 95)}%`, up: true },
    { title: "Users", value: stats.userCount.toLocaleString("en-US"), trend: `${stats.subscriptionCount} plans`, icon: "img/icon/visitor.svg", color: "teal", width: `${Math.min(stats.userCount * 22, 95)}%`, up: true },
    { title: "Revenue", value: `${Number(stats.revenue || 0).toLocaleString("en-US")} RUB`, trend: `${stats.averageRating} avg rating`, icon: "img/icon/browser.svg", color: "indigo", width: `${Math.min(Number(stats.revenue || 0) / 10, 95)}%`, up: true },
  ];

  return (
    <>
      <h1 className="page-header mb-3">
        Hi, admin. <small>here's what's happening with Zetflix today.</small>
      </h1>

      <div className="d-flex justify-content-end mb-3">
        <button type="button" className="btn btn-outline-primary btn-sm" onClick={loadDashboard} disabled={isRefreshing}>
          <i className={`fa ${isRefreshing ? "fa-spinner fa-spin" : "fa-rotate"} me-2`}></i>
          Refresh stats
        </button>
      </div>

      {loadError && (
        <div className="alert alert-warning py-2">
          <i className="fa fa-triangle-exclamation me-2"></i>{loadError}
        </div>
      )}

      <div className="row">
        <div className="col-xl-6 mb-3">
          <WeeklyEarning stats={stats} onViewRevenue={onViewRevenue} />
        </div>
        <div className="col-xl-6">
          <div className="row">
            {dashboardCards.map((card) => <GradientMetric key={card.title} card={card} />)}
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-xl-6">
          <div className="row">
            <div className="col-sm-6 mb-3 d-flex flex-column">
              <SmallStatCard stats={stats} />
              <SocialCard stats={stats} />
            </div>
            <div className="col-sm-6 mb-3">
              <TrafficCard stats={stats} />
            </div>
          </div>
        </div>
        <div className="col-xl-6 mb-3">
          <SalesAnalytics stats={stats} />
        </div>
      </div>
    </>
  );
}

export default function StudioAdmin() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [activePage, setActivePage] = useState("dashboard");
  const [adminSession, setAdminSession] = useState(getStoredAdminSession);
  const [reviewCount, setReviewCount] = useState(0);

  async function refreshReviewCount() {
    try {
      const response = await adminFetch(`${apiBaseUrl}/api/admin/reviews`);
      const reviews = await response.json();
      setReviewCount(Array.isArray(reviews) ? reviews.length : 0);
    } catch (error) {
      setReviewCount(0);
    }
  }

  useEffect(() => {
    function handleUnauthorized() {
      setAdminSession(null);
      setActivePage("dashboard");
      setSidebarOpen(false);
    }

    window.addEventListener(adminUnauthorizedEvent, handleUnauthorized);

    return () => {
      window.removeEventListener(adminUnauthorizedEvent, handleUnauthorized);
    };
  }, []);

  useEffect(() => {
    let isMounted = true;

    async function validateSession() {
      if (!adminSession?.token) {
        return;
      }

      try {
        const response = await adminFetch(`${apiBaseUrl}/api/admin/auth/session`);

        if (!response.ok) {
          throw new Error(`Session returned ${response.status}`);
        }
      } catch (error) {
        localStorage.removeItem(adminSessionStorageKey);
        if (isMounted) {
          setAdminSession(null);
          setActivePage("dashboard");
          setSidebarOpen(false);
        }
      }
    }

    validateSession();

    return () => {
      isMounted = false;
    };
  }, [adminSession?.token]);

  useEffect(() => {
    if (adminSession?.token) {
      refreshReviewCount();
    } else {
      setReviewCount(0);
    }
  }, [adminSession?.token]);

  const renderPage = () => {
    if (activePage === "dashboard") return <DashboardPage onViewRevenue={() => handleSelect("analytics")} />;
    if (activePage === "movies") return <MoviesPage />;
    if (activePage === "shows") return <ShowsPage />;
    if (activePage === "genres") return <GenresPage />;
    if (activePage === "episodes") return <EpisodesPage />;
    if (activePage === "media") return <MediaPage />;
    if (activePage === "analytics") return <AnalyticsPage />;
    if (activePage === "users") return <UsersPage />;
    if (activePage === "subscriptions") return <SubscriptionsPage />;
    if (activePage === "audit") return <AuditLogsPage />;
    if (activePage === "settings") return <SettingsPage />;
    if (activePage === "reviews") return <ReviewsPage onReviewCountChange={setReviewCount} />;
    return <PlaceholderPage title={sidebarGroups.flatMap((group) => group.items).find((item) => item.page === activePage)?.label || "Section"} />;
  };

  const handleSelect = (page) => {
    setActivePage(page);
    setSidebarOpen(false);
  };

  const handleLogout = async () => {
    try {
      await adminFetch(`${apiBaseUrl}/api/admin/auth/logout`, { method: "POST" });
    } catch (error) {
      // Local logout still clears the dashboard session.
    }

    localStorage.removeItem(adminSessionStorageKey);
    setAdminSession(null);
  };

  if (!adminSession?.token) {
    return <AdminLoginPage onLogin={setAdminSession} />;
  }

  return (
    <div id="app" className={`app ${sidebarOpen ? "app-sidebar-mobile-toggled" : ""}`}>
      <Header
        onToggle={() => setSidebarOpen((value) => !value)}
        adminSession={adminSession}
        onLogout={handleLogout}
        onOpenReviews={() => handleSelect("reviews")}
        reviewCount={reviewCount}
      />
      <Sidebar activePage={activePage} mobileOpen={sidebarOpen} onSelect={handleSelect} reviewCount={reviewCount} />
      <div id="content" className="app-content">
        {renderPage()}
      </div>
      <button type="button" className="theme-collapse-btn studio-floating-cog" aria-label="Theme panel">
        <i className="fa fa-cog"></i>
      </button>
    </div>
  );
}
