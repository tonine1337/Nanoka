import React from 'react';
import { BrowserRouter as Router, Route, NavLink } from "react-router-dom";
import Index from './Index/Index';

function App() {
  return (
    <Router>
      <div>
        <div>
          <div className="ui menu">
            <a href="/" className="header item">Nanoka</a>
            <div className="ui simple dropdown item">
              <i className="icon book"></i>
              Doujinshi
              <div className="menu" style={{ minWidth: "15em" }}>
                <div className="header">Doujinshi</div>
                <NavLink to="/doujinshi/listby/name" className="item"><i className="icon sort alphabet down"></i> Name</NavLink>
                <NavLink to="/doujinshi/listby/upload" className="item"><i className="icon clock"></i> Newest</NavLink>
                <NavLink to="/doujinshi/listby/trending" className="item"><i className="icon chart line"></i> Trending</NavLink>
                <NavLink to="/doujinshi/listby/viewed" className="item"><i className="icon eye"></i> Most Viewed</NavLink>
                <NavLink to="/doujinshi/random" className="item"><i className="icon random"></i> Random</NavLink>
                <div className="divider"></div>

                <div className="header">Meta</div>
                <NavLink to="/doujinshi/listmeta/artist" className="item"><i className="icon paint brush"></i> Artist</NavLink>
                <NavLink to="/doujinshi/listmeta/group" className="item"><i className="icon users"></i> Group</NavLink>
                <NavLink to="/doujinshi/listmeta/parody" className="item"><i className="icon linkify"></i> Parody</NavLink>
                <NavLink to="/doujinshi/listmeta/character" className="item"><i className="icon odnoklassniki"></i> Character</NavLink>
                <NavLink to="/doujinshi/listmeta/category" className="item"><i className="icon th large"></i> Category</NavLink>
                <NavLink to="/doujinshi/listmeta/language" className="item"><i className="icon globe"></i> Language</NavLink>
                <NavLink to="/doujinshi/listmeta/tag" className="item"><i className="icon tag"></i> Tag</NavLink>
                <NavLink to="/doujinshi/listmeta/convention" className="item"><i className="icon calendar"></i> Convention</NavLink>
              </div>
            </div>
            <div className="ui simple dropdown item">
              <i className="icon image"></i>
              Booru
              <div className="menu" style={{ minWidth: "15em" }}>
                <div className="header">Booru</div>
                <NavLink to="/booru/listby/upload" className="item"><i className="icon clock"></i> Newest</NavLink>
                <NavLink to="/booru/listby/trending" className="item"><i className="icon chart line"></i> Trending</NavLink>
                <NavLink to="/booru/listby/viewed" className="item"><i className="icon eye"></i> Most Viewed</NavLink>
                <NavLink to="/booru/random" className="item"><i className="icon random"></i> Random</NavLink>
                <div className="divider"></div>

                <div className="header">Tag</div>
                <NavLink to="/booru/listtag/general" className="item"><i className="icon tag"></i> General</NavLink>
                <NavLink to="/booru/listtag/artist" className="item"><i className="icon paint brush"></i> Artist</NavLink>
                <NavLink to="/booru/listtag/character" className="item"><i className="icon odnoklassniki"></i> Character</NavLink>
                <NavLink to="/booru/listtag/copyright" className="item"><i className="icon copyright"></i> Copyright</NavLink>
                <NavLink to="/booru/listtag/metadata" className="item"><i className="icon info"></i> Metadata</NavLink>
              </div>
            </div>
            <div className="ui simple dropdown item">
              <i className="icon users"></i>
              Community
              <div className="menu" style={{ minWidth: "15em" }}>
                <div className="header">Activity</div>
                <NavLink to="/activity/edit" className="item"><i className="icon edit"></i> Edits</NavLink>
                <NavLink to="/activity/comment" className="item"><i className="icon comments"></i> Comments</NavLink>
                <NavLink to="/activity/note" className="item"><i className="icon sticky note"></i> Notes</NavLink>
                <NavLink to="/activity/moderation" className="item"><i className="icon user secret"></i> Moderation</NavLink>
                <div className="divider"></div>

                <div className="header">Wiki</div>
                <NavLink to="/wiki" className="item"><i className="icon book"></i> Wiki</NavLink>
                <div className="divider"></div>

                <div className="header">Forum</div>
                <a href="https://reddit.com" target="_blank" className="item"><i className="icon reddit"></i> Reddit</a>
                <a href="https://discord.gg/Edx7Rbc" target="_blank" className="item"><i className="icon discord"></i> Discord</a>
              </div>
            </div>
            <div className="right menu">
              <div className="item">
                <div className="ui action left icon input">
                  <i className="search icon"></i>
                  <input type="text" style={{ minWidth: "32rem" }} />
                  <button className="ui button primary">Search</button>
                </div>
              </div>
              <div className="ui simple dropdown item">
                <i className="icon cog"></i>
                <div className="menu" style={{ minWidth: "15em", marginRight: "2px" }}>
                  <div className="header">Nanoka</div>
                  <NavLink to="/account/profile" className="item"><i className="icon user"></i> Profile</NavLink>
                  <NavLink to="/account/dashboard" className="item"><i className="icon dashboard"></i> Dashboard</NavLink>
                  <NavLink to="/account/uploads" className="item"><i className="icon upload"></i> My uploads</NavLink>
                  <NavLink to="/settings/language" className="item"><i className="icon globe"></i> Language</NavLink>
                  <NavLink to="/settings/theme" className="item"><i className="icon theme"></i> Theme</NavLink>
                  <div className="divider"></div>

                  <div className="header">Misc.</div>
                  <a href="https://github.com/chiyadev/Nanoka" target="_blank" className="item"><i className="icon github"></i> Open Source</a>
                  <a href="https://github.com/chiyadev/Nanoka" target="_blank" className="item"><i className="icon heart"></i> Support Nanoka</a>
                  <div className="divider"></div>

                  <div className="header">Management</div>
                  <NavLink to="/management" className="item"><i className="icon database"></i> Database</NavLink>
                  <a href="/client" className="item"><i className="icon terminal"></i> Open client</a>
                  <a href="/signout" className="item"><i className="icon sign-out"></i> Sign out</a>
                </div>
              </div>
            </div>
          </div>
        </div>

        <main className="ui container" style={{ marginTop: "3rem", marginBottom: "3rem" }}>
          <Route path="/" exact component={Index} />
        </main>
      </div>
    </Router>
  );
}

export default App;
