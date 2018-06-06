using System;

public enum GameStatus
{
    /// <summary>
    /// The match is scheduled to be played
    /// </summary>
    Scheduled,
    /// <summary>
    /// The match is currently in progress
    /// </summary>
    InProgress,
	PreGame,
    /// <summary>
    /// The match has been postponed to a future date
    /// </summary>
    Postponed,
    /// <summary>
    /// The match has been temporarily delayed and will be continued
    /// </summary>
    Delayed,
    /// <summary>
    /// The match has been canceled and will not be played
    /// </summary>
    Canceled,
	/// <summary>
	/// The match is at a pause for halftime
	/// </summary>
	Halftime,
    /// <summary>
    /// The match is over
    /// </summary>
    Closed,
	Complete,
	Final,
    /// <summary>
    /// The status is not recognized
    /// </summary>
    Unknown
}

